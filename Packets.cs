namespace Server
{
    public enum ServerPackets
    {
        HandShake = 1,
        UserInfoRequest = 2
    }

    public enum ClientPackets
    {
        HandShakeReceived = 1,
        UserInfoRequestReceived = 2
    }
}