using System.Security.Cryptography;
using DormitoryManagement.Application.Abstractions.Auth;

namespace DormitoryManagement.Application.Services.Auth;

public sealed class NumericOtpGenerator : IOtpGenerator
{
    public string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}
