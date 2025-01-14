using Domain.DTOs;

namespace Domain.Entities
{
    public class Deck
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public List<DeckCard> DeckCards { get; set; }
        public bool IsPublic { get; set; }
        public User User { get; set; }

        // New field to store comments
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public DeletedDeck? DeletedDeck { get; set; }

        public static Deck ToEntity(DeckDTO deckDto)
        {

            var deck = new Deck
            {
                Id = deckDto.Id,
                UserId = deckDto.UserId,
                Name = deckDto.Name,
                IsPublic = deckDto.IsPublic,
                Comments = deckDto.Comments.Select(commentDto => Comment.ToEntity(commentDto)).ToList()
            };
            deck.DeckCards = deckDto.Cards.Select(card => new DeckCard
            {
                CardId = card.Id,
                Deck = deck
            }).ToList();

            return deck;
        }
        public static DeckDTO FromEntity(Deck deck)
        {
            return new DeckDTO
            {
                Id = deck.Id,
                UserId = deck.UserId,
                UserName = deck.User.Username,
                Name = deck.Name,
                Cards = deck.DeckCards.Select(dc => Card.FromEntity(dc.Card)).ToList(),
                IsPublic = deck.IsPublic,
                Comments = deck.Comments.Select(comment => Comment.FromEntity(comment)).ToList()
            };
        }

    }
}
