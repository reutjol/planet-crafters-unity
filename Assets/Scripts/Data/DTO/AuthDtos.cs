using System;
using System.Collections.Generic;

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
    public string userName;
    public string email;
    public int coins;
    public string selectedAvatar;
    public List<string> ownedAvatars;
}
