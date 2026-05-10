# com.gamecore.trade

Negotiation loop between players or with the bank. Legality is delegated to an injected `ITradeRule` so the package contains no game-specific logic.

## What it does

`TradeManager` manages the lifecycle of trade offers: propose → accept/reject/counter → complete/cancel. It enforces nothing itself — `ITradeRule.CanTrade` is called before any resources move. This separation means the same `TradeManager` works for any game that uses `ResourceBundle`.

## Key types

| Type | Role |
|---|---|
| `TradeStatus` | Enum: Pending / Accepted / Rejected / Countered / Cancelled |
| `ITradeOffer` | Read-only view of a trade offer |
| `TradeOffer` | Concrete mutable offer; `Status` is settable internally |
| `ITradeRule` | Validates whether a trade is legal; implemented by Catan |
| `TradeManager` | MonoBehaviour; inject `Rule` before first trade |

## Events published

`TradeProposedEvent`, `TradeAcceptedEvent`, `TradeRejectedEvent`, `TradeCompletedEvent`, `TradeCancelledEvent`

## Dependencies

`com.gamecore.events`, `com.gamecore.player`, `com.gamecore.resources`

## Extension point

`CatanTradeRule` (in `Assets/Catan/Rules/`) enforces the trading phase gate and port ratios for bank trades.
