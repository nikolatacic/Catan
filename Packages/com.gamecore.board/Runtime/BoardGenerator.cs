namespace GameCore.Board
{
    public abstract class BoardGenerator<T> where T : IHexTile
    {
        public abstract HexGrid<T> Generate();
    }
}
