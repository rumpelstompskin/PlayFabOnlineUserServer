namespace Server
{
    public enum ServerPackets
    {
        HandShake = 1,
        UserInfoRequest = 2,
        AuthorizeClient = 3
    }

    public enum ClientPackets
    {
        HandShakeReceived = 1,
        UserInfoRequestReceived = 2,
        AuthorizeClientReceived = 3
    }
}