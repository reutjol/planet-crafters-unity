using System;

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
