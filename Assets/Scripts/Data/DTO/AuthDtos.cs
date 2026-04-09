using System;

/// <summary>
/// Data Transfer Objects for authentication API responses.
/// </summary>

[Serializable]
public class AuthResponseDto
{
    public string accessToken;
    public string refreshToken;
    public UserDto user;
}

[Serializable]
public class UserDto
{
    public string id;
    public string name;
    public string email;
}
