namespace DormitoryManagement.Application.Abstractions.Auth;

public interface IOtpGenerator
{
    string GenerateCode();
}
