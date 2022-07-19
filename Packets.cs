namespace Server
{
    public enum ServerPackets
    {
        HandShake = 1,
        UserInfoRequest = 2,
        AuthorizeClient = 3,
        FriendRequest = 4,
        FriendResponse = 5
    }

    public enum ClientPackets
    {
        HandShakeReceived = 1,
        UserInfoRequestReceived = 2,
        AuthorizeClientReceived = 3,
        FriendsRequestReceived = 4,
        FriendRequestResponseReceived = 5
    }
}