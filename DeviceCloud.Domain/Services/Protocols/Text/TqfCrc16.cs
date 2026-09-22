using System.Text;

namespace DeviceCloud.Domain.Services.Protocols.Text;

/// <summary>
/// TQF 自定义 CRC16（文本协议指令校验）。
/// 从 WpfApp1 SerialCommunicationService.getCRC / cal_crc_half 移植。
/// 查表法（16 项），结果高位在前，并对 0x28/0x0D/0x0A 做 +1 规避（避免与 '('、CR、LF 冲突）。
/// </summary>
public static class TqfCrc16
{
    private static readonly int[] CrcTable = new int[16]
    {
        0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7,
        0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef
    };

    /// <summary>
    /// 计算 TQF CRC16，返回 2 字节（高位在前）。
    /// </summary>
    public static byte[] GetCrc(byte[] data)
    {
        int value = CalcCrcHalf(data, data.Length);
        return new byte[] { (byte)(value >> 8), (byte)(value & 0xff) };
    }

    private static int CalcCrcHalf(byte[] pin, int len)
    {
        int crc = 0;
        int i = 0;

        while (len-- != 0)
        {
            byte da = (byte)(((byte)(crc >> 8)) >> 4);
            crc <<= 4;
            crc ^= CrcTable[(da ^ (pin[i] >> 4))];

            da = (byte)(((byte)(crc >> 8)) >> 4);
            crc <<= 4;
            crc ^= CrcTable[(da ^ (pin[i] & 0x0f))];
            i++;
        }

        byte crcLow = (byte)crc;
        byte crcHigh = (byte)(crc >> 8);

        if (crcLow == 0x28 || crcLow == 0x0d || crcLow == 0x0a) crcLow++;
        if (crcHigh == 0x28 || crcHigh == 0x0d || crcHigh == 0x0a) crcHigh++;

        return (crcHigh << 8) + crcLow;
    }

    /// <summary>
    /// 组装文本指令字节：ASCII(前缀+值) + CRC + CR。
    /// </summary>
    public static byte[] BuildFrame(string command)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(command);
        byte[] crc = GetCrc(bytes);

        var result = new byte[bytes.Length + crc.Length + 1];
        Array.Copy(bytes, 0, result, 0, bytes.Length);
        Array.Copy(crc, 0, result, bytes.Length, crc.Length);
        result[result.Length - 1] = 0x0D; // \r
        return result;
    }

    /// <summary>
    /// 字节数组转十六进制字符串（用于透传下发）。
    /// </summary>
    public static string ToHexString(byte[] data)
        => Convert.ToHexString(data);
}
