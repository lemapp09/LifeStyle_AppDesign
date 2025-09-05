using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AppDesign
{
    public class SolitaireGameManager : MonoBehaviour
    {
        // Reference to the PlayingCards_SO ScriptableObject
        [SerializeField] private PlayingCards_SO _playingCards;

        // UI Element variables
        private VisualElement _solitaireGameScreen;
        private Button _solitaireDeckButton;
        private VisualElement _solitaireDiscardPile;
        private List<Button> _solitaireFoundationPiles = new List<Button>();
        private List<Button> _solitaireTableauPiles = new List<Button>();

        // Script-wide variables
        private List<CardData> _deck = new List<CardData>();
        private List<CardData> _discardPile = new List<CardData>();
        private List<List<CardData>> _foundationPiles = new List<List<CardData>>();
        private List<List<CardData>> _tableauPiles = new List<List<CardData>>();
        private CardData _selectedCard = null;
        private VisualElement _selectedCardUI = null;
        private List<CardData> _selectedCardSourcePile = null;
        private WiggleEffect _wiggleEffect;

        // Method to set UI Element references from AppManager
        public void SetUIElements(VisualElement gameScreen, Button deckButton, VisualElement discardPile, 
                                List<Button> foundationPiles, List<Button> tableauPiles,
                                WiggleEffect wiggleEffect)
        {
            _solitaireGameScreen = gameScreen;
            _solitaireDeckButton = deckButton;
            _solitaireDiscardPile = discardPile;
            _solitaireFoundationPiles = foundationPiles;
            
            if (tableauPiles.Count != 7)
            {
                Debug.LogError($"Expected 7 tableau piles but received {tableauPiles.Count}. Please ensure your AppManager provides 7 Buttons for the Solitaire game.");
            }
            _solitaireTableauPiles = tableauPiles;
            _wiggleEffect = wiggleEffect;

            _solitaireDeckButton.clickable.clicked += OnDeckClicked;
        }

        public void InitializeGame()
        {
            // Clear all existing piles and lists before starting a new game
            _deck.Clear();
            _discardPile.Clear();
            _foundationPiles.Clear();
            _tableauPiles.Clear();
            
            // Reset the selection state
            ResetSelection();

            // Re-initialize the deck
            _deck = new List<CardData>(_playingCards.StandardCards);
            Shuffle(_deck);

            // Create the empty foundation and tableau piles
            for (int i = 0; i < 4; i++)
            {
                _foundationPiles.Add(new List<CardData>());
            }
            for (int i = 0; i < 7; i++)
            {
                _tableauPiles.Add(new List<CardData>());
            }

            DealCards();
            UpdateUI();
        }

        private void Shuffle(List<CardData> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (_deck[i], _deck[j]) = (_deck[j], _deck[i]);
            }
        }

        private void DealCards()
        {
            int cardsDealt = 0;
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    _tableauPiles[i].Add(_deck[cardsDealt]);
                    cardsDealt++;
                }
            }
            _deck.RemoveRange(0, cardsDealt);
        }

        private void UpdateUI()
        {
            _solitaireDeckButton.style.backgroundImage = null;
            _solitaireDiscardPile.Clear();
            _solitaireFoundationPiles.ForEach(p => p.Clear());
            _solitaireTableauPiles.ForEach(p => p.Clear());

            if (_deck.Count > 0)
            {
                _solitaireDeckButton.style.backgroundImage = new StyleBackground(_playingCards.SpecialCards[0].cardSprite);
            }

            if (_discardPile.Count > 0)
            {
                CardData topCard = _discardPile[_discardPile.Count - 1];
                Button cardButton = new Button();
                cardButton.style.backgroundImage = new StyleBackground(topCard.cardSprite);
                cardButton.style.position = new StyleEnum<Position>(Position.Absolute);
                cardButton.style.width = 115;
                cardButton.style.height = 167;
                cardButton.style.top = 0;
                cardButton.style.left = 0;
                cardButton.clicked += () => OnCardClicked(cardButton, topCard, _discardPile);
                cardButton.RegisterCallback<PointerEnterEvent>(_wiggleEffect.OnHoverEnter);
                cardButton.RegisterCallback<PointerLeaveEvent>(_wiggleEffect.OnHoverLeave);
                _solitaireDiscardPile.Add(cardButton);
            }

            for (int i = 0; i < _tableauPiles.Count; i++)
            {
                Button tableauPileUI = _solitaireTableauPiles[i];
                var i1 = i;
                
                tableauPileUI.clicked += () => OnPileClicked(_tableauPiles[i1]);

                for (int j = 0; j < _tableauPiles[i].Count; j++)
                {
                    CardData card = _tableauPiles[i][j];
                    VisualElement cardElement = new VisualElement();
                    
                    if (j == _tableauPiles[i].Count - 1)
                    {
                        Button cardButton = new Button();
                        cardButton.style.backgroundImage = new StyleBackground(card.cardSprite);
                        cardButton.style.position = new StyleEnum<Position>(Position.Absolute);
                        cardButton.style.top = j * 30;
                        cardButton.style.width = 100;
                        cardButton.style.height = 150;
                        cardButton.clicked += () => OnCardClicked(cardButton, card, _tableauPiles[i1]);
                        tableauPileUI.Add(cardButton);
                    }
                    else
                    {
                        cardElement.style.backgroundImage = new StyleBackground(_playingCards.SpecialCards[0].cardSprite);
                        cardElement.style.position = new StyleEnum<Position>(Position.Absolute);
                        cardElement.style.top = j * 30;
                        cardElement.style.width = 100;
                        cardElement.style.height = 150;
                        tableauPileUI.Add(cardElement);
                    }
                }
            }
            
            for (int i = 0; i < _foundationPiles.Count; i++)
            {
                Button foundationPileUI = _solitaireFoundationPiles[i];
                var i1 = i;
                
                foundationPileUI.clicked += () => OnPileClicked(_foundationPiles[i1]);
                
                if (_foundationPiles[i].Count > 0)
                {
                    CardData card = _foundationPiles[i].Last();
                    Button cardButton = new Button();
                    cardButton.style.backgroundImage = new StyleBackground(card.cardSprite);
                    cardButton.style.width = 100;
                    cardButton.style.height = 150;
                    cardButton.clicked += () => OnCardClicked(cardButton, card, _foundationPiles[i1]);
                    foundationPileUI.Add(cardButton);
                }
            }
        }

        private void OnDeckClicked()
        {
            if (_deck.Count > 0)
            {
                CardData card = _deck[_deck.Count - 1];
                _deck.RemoveAt(_deck.Count - 1);
                _discardPile.Add(card);
            }
            else
            {
                _deck = new List<CardData>(_discardPile);
                _deck.Reverse();
                _discardPile.Clear();
            }
            UpdateUI();
        }

        private void OnCardClicked(Button clickedCardUI, CardData clickedCard, List<CardData> sourcePile)
        {
            if (_selectedCard == null)
            {
                _selectedCard = clickedCard;
                _selectedCardUI = clickedCardUI;
                _selectedCardSourcePile = sourcePile;
                _selectedCardUI.style.borderTopWidth = 5;
                _selectedCardUI.style.borderRightWidth = 5;
                _selectedCardUI.style.borderBottomWidth = 5;
                _selectedCardUI.style.borderLeftWidth = 5;
                _selectedCardUI.style.borderTopColor = new StyleColor(Color.yellow);
                _selectedCardUI.style.borderRightColor = new StyleColor(Color.yellow);
                _selectedCardUI.style.borderBottomColor = new StyleColor(Color.yellow);
                _selectedCardUI.style.borderLeftColor = new StyleColor(Color.yellow);
            }
            else
            {
                if (_selectedCard == clickedCard)
                {
                    ResetSelection();
                }
                else
                {
                    if (IsValidMove(_selectedCard, _selectedCardSourcePile, clickedCard))
                    {
                        MoveCard(_selectedCard, _selectedCardSourcePile, clickedCard);
                        ResetSelection();
                        UpdateUI();
                    }
                    else
                    {
                        Debug.Log("Invalid move!");
                        ResetSelection();
                    }
                }
            }
        }

        private void OnPileClicked(List<CardData> destinationPile)
        {
            if (_selectedCard != null)
            {
                if (IsValidMove(_selectedCard, _selectedCardSourcePile, null))
                {
                    MoveCard(_selectedCard, _selectedCardSourcePile, null);
                    ResetSelection();
                    UpdateUI();
                }
                else
                {
                    Debug.Log("Invalid move to empty pile!");
                    ResetSelection();
                }
            }
        }

        private void ResetSelection()
        {
            _selectedCard = null;
            _selectedCardSourcePile = null;
            if (_selectedCardUI != null)
            {
                _selectedCardUI.style.borderTopWidth = 0;
                _selectedCardUI.style.borderRightWidth = 0;
                _selectedCardUI.style.borderBottomWidth = 0;
                _selectedCardUI.style.borderLeftWidth = 0;
                _selectedCardUI = null;
            }
        }

        private bool IsValidMove(CardData cardToMove, List<CardData> sourcePile, CardData destinationCard)
        {
            // Allow a card to be moved to an empty pile.
            if (destinationCard == null)
            {
                 // A King can be moved to an empty tableau pile.
                 if (_tableauPiles.Any(p => p.Count == 0) && cardToMove.cardValue == CardValue.King)
                 {
                    return true;
                 }
                 // An Ace can be moved to an empty foundation pile.
                 if (_foundationPiles.Any(p => p.Count == 0) && cardToMove.cardValue == CardValue.Ace)
                 {
                     return true;
                 }
            }
            
            // Tableau to Tableau move
            if (_tableauPiles.Any(p => p.Contains(cardToMove)) && _tableauPiles.Any(p => p.Contains(destinationCard)))
            {
                if ((int)destinationCard.cardValue - 1 == (int)cardToMove.cardValue && IsOppositeColor(cardToMove, destinationCard))
                {
                    return true;
                }
            }
            
            // Tableau to Foundation move
            if (_tableauPiles.Any(p => p.Contains(cardToMove)) && _foundationPiles.Any(p => p.Contains(destinationCard)))
            {
                 if (cardToMove.cardSuit == destinationCard.cardSuit && (int)cardToMove.cardValue == (int)destinationCard.cardValue + 1)
                 {
                    return true;
                 }
            }
            
            // Discard to Tableau move
            if (_discardPile.Contains(cardToMove) && _tableauPiles.Any(p => p.Contains(destinationCard)))
            {
                 if ((int)destinationCard.cardValue - 1 == (int)cardToMove.cardValue && IsOppositeColor(cardToMove, destinationCard))
                 {
                    return true;
                 }
            }
            
            // Discard to Foundation move
            if (_discardPile.Contains(cardToMove) && _foundationPiles.Any(p => p.Contains(destinationCard)))
            {
                 if (cardToMove.cardSuit == destinationCard.cardSuit && (int)cardToMove.cardValue == (int)destinationCard.cardValue + 1)
                 {
                    return true;
                 }
            }
            
            // Foundation to Tableau move
            if (_foundationPiles.Any(p => p.Contains(cardToMove)) && _tableauPiles.Any(p => p.Contains(destinationCard)))
            {
                 if ((int)destinationCard.cardValue - 1 == (int)cardToMove.cardValue && IsOppositeColor(cardToMove, destinationCard))
                 {
                    return true;
                 }
            }
            
            // Foundation to Foundation move
            if (_foundationPiles.Any(p => p.Contains(cardToMove)) && _foundationPiles.Any(p => p.Contains(destinationCard)))
            {
                 if (cardToMove.cardSuit == destinationCard.cardSuit && (int)cardToMove.cardValue == (int)destinationCard.cardValue + 1)
                 {
                    return true;
                 }
            }

            return false;
        }
        
        private void MoveCard(CardData cardToMove, List<CardData> sourcePile, CardData destinationCard)
        {
            sourcePile.Remove(cardToMove);
            
            if (destinationCard == null)
            {
                // Move to an empty pile
                if (_tableauPiles.Any(p => p.Count == 0))
                {
                    _tableauPiles.First(p => p.Count == 0).Add(cardToMove);
                }
                else if (_foundationPiles.Any(p => p.Count == 0))
                {
                    _foundationPiles.First(p => p.Count == 0).Add(cardToMove);
                }
            }
            // Find the correct destination pile and add the card
            else if (_tableauPiles.Any(p => p.Contains(destinationCard)))
            {
                _tableauPiles.First(p => p.Contains(destinationCard)).Add(cardToMove);
            }
            else if (_foundationPiles.Any(p => p.Contains(destinationCard)))
            {
                _foundationPiles.First(p => p.Contains(destinationCard)).Add(cardToMove);
            }
        }
        
        private bool IsOppositeColor(CardData card1, CardData card2)
        {
            bool isBlack1 = card1.cardSuit == CardSuit.Clubs || card1.cardSuit == CardSuit.Spades;
            bool isBlack2 = card2.cardSuit == CardSuit.Clubs || card2.cardSuit == CardSuit.Spades;
            return isBlack1 != isBlack2;
        }
    }
}