namespace DeviceCloud.Domain.Services.Protocols.Modbus;

/// <summary>
/// Modbus RTU CRC16 校验。
/// 从 WpfApp1.Convert.ModbusRTU.CRC16 移植（多项式 0xA001）。
/// </summary>
public static class ModbusCrc16
{
    /// <summary>
    /// 计算 Modbus RTU CRC16（多项式 0xA001，初始值 0xFFFF）。
    /// </summary>
    /// <param name="data">参与计算的数据</param>
    /// <param name="length">参与计算的字节长度</param>
    /// <returns>CRC16 值（低字节在前时按小端处理由调用方决定）</returns>
    public static ushort Compute(byte[] data, int length)
    {
        ushort crc = 0xFFFF;

        for (int i = 0; i < length; i++)
        {
            crc ^= data[i];
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ 0xA001);
                else
                    crc >>= 1;
            }
        }

        return crc;
    }

    /// <summary>
    /// 计算完整数组的 CRC16。
    /// </summary>
    public static ushort Compute(byte[] data) => Compute(data, data.Length);
}
