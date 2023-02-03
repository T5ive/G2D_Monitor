namespace G2D_Monitor.Manager
{
    public enum GameState : ushort
    {
        InLobby,
        Drafting, //轮抽
        InGame,
        Opening,
        Discussion,
        Voting,
        Waiting, //投票结算
        Proceeding, //放逐界面
        Unknown
    }
}
