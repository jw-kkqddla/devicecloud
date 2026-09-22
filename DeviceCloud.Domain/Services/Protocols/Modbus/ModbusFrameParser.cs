namespace DeviceCloud.Domain.Services.Protocols.Modbus;

/// <summary>
/// Modbus RTU 帧解析。
/// 从 WpfApp1.Convert.ModbusRTU 移植（ParseRead03Response / ParseRead20Response / GetBits），
/// 剥离 showStatue 回调与串口依赖。
/// </summary>
public static class ModbusFrameParser
{
    /// <summary>
    /// 解析功能码 0x03 返回帧，提取寄存器数据（小端在前）。
    /// 帧结构：[从站地址][0x03][字节数][数据...][CRC低][CRC高]。
    /// </summary>
    /// <param name="response">从机返回的完整字节数组</param>
    /// <returns>寄存器值数组；帧不合法或 CRC 校验失败时返回 null</returns>
    public static short[]? ParseRead03Response(byte[]? response)
    {
        if (response is null || response.Length < 5)
            return null;

        byte funcCode = response[1];
        byte byteCount = response[2];

        if (funcCode != 0x03)
            return null;

        if (response.Length < 3 + byteCount + 2)
            return null;

        // CRC 校验（低字节在前）
        ushort recvCrc = (ushort)(response[response.Length - 2] | (response[response.Length - 1] << 8));
        ushort calcCrc = ModbusCrc16.Compute(response, response.Length - 2);
        if (recvCrc != calcCrc)
            return null;

        int regCount = byteCount / 2;
        var registers = new short[regCount];

        for (int i = 0; i < regCount; i++)
        {
            int dataIndex = 3 + i * 2;
            // 数据为大端格式（高字节在前），按小端在前返回
            registers[i] = (short)((response[dataIndex + 1] << 8) | response[dataIndex]);
        }

        return registers;
    }

    /// <summary>
    /// 解析功能码 0x20 返回帧，提取寄存器数据（小端在前）。
    /// </summary>
    public static short[]? ParseRead20Response(byte[]? response)
    {
        if (response is null || response.Length < 5)
            return null;

        byte funcCode = response[1];
        byte byteCount = response[2];

        if (funcCode != 0x20)
            return null;

        if (response.Length < 3 + byteCount + 2)
            return null;

        ushort recvCrc = (ushort)(response[response.Length - 2] | (response[response.Length - 1] << 8));
        ushort calcCrc = ModbusCrc16.Compute(response, response.Length - 2);
        if (recvCrc != calcCrc)
            return null;

        int regCount = byteCount / 2;
        var registers = new short[regCount];

        for (int i = 0; i < regCount; i++)
        {
            int dataIndex = 3 + i * 2;
            registers[i] = (short)((response[dataIndex + 1] << 8) | response[dataIndex]);
        }

        return registers;
    }

    /// <summary>
    /// 获取 16 位的位值（bit0 为最低位）。
    /// </summary>
    public static int[] GetBits(short value)
    {
        var bits = new int[16];
        for (int i = 0; i < 16; i++)
        {
            bits[i] = (value >> i) & 1;
        }
        return bits;
    }
}
