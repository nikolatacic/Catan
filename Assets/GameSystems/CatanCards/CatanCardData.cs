/// <summary>
/// Catan-specific card data for development cards.
/// Extends the generic CardData with Catan-specific information.
/// </summary>
public class CatanCardData : CardData
{
    /// <summary>
    /// The Catan-specific card type.
    /// </summary>
    public CatanCardType CatanType { get; private set; }

    /// <summary>
    /// Whether this card can be played immediately after drawing (Knight only).
    /// </summary>
    public bool CanPlayImmediately { get; private set; }

    /// <summary>
    /// Whether this card is a victory point card (hidden until end of game).
    /// </summary>
    public bool IsVictoryPoint { get; private set; }

    /// <summary>
    /// Create a new CatanCardData instance.
    /// </summary>
    /// <param name="catanType">Catan card type</param>
    public CatanCardData(CatanCardType catanType) : base(catanType.ToString(), GetCardName(catanType))
    {
        CatanType = catanType;
        CanPlayImmediately = catanType == CatanCardType.Knight;
        IsVictoryPoint = catanType == CatanCardType.VictoryPoint;
    }

    /// <summary>
    /// Get the display name for a Catan card type.
    /// </summary>
    private static string GetCardName(CatanCardType type)
    {
        switch (type)
        {
            case CatanCardType.Knight:
                return "Knight";
            case CatanCardType.RoadBuilding:
                return "Road Building";
            case CatanCardType.YearOfPlenty:
                return "Year of Plenty";
            case CatanCardType.Monopoly:
                return "Monopoly";
            case CatanCardType.VictoryPoint:
                return "Victory Point";
            default:
                return type.ToString();
        }
    }

    /// <summary>
    /// Create a copy of this card.
    /// </summary>
    public new CatanCardData Clone()
    {
        return new CatanCardData(CatanType);
    }

    /// <summary>
    /// Get a string representation of the card.
    /// </summary>
    public override string ToString()
    {
        return $"CatanCard[{CardID}]: {CardName} ({CatanType})";
    }
}

