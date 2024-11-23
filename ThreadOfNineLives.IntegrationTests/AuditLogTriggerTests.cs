﻿using System.Data;
using Infrastructure.Persistance.Relational;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Persistance;

namespace ThreadOfNineLives.IntegrationTests
{
    public class AuditLogTriggerTests : IDisposable
    {
        private readonly string _connectionString;
        private readonly RelationalContext _db;

        private Card _testCard;
        private Comment _testComment;
        private Deck _testDeck;
        private DeckCard _testDeckCard;
        private Enemy _testEnemy;
        private Fight _testFight;
        private GameAction _testGameAction;
        private User _testUser;

        public AuditLogTriggerTests()
        {
            _connectionString = PersistanceConfiguration.GetConnectionString(dbtype.Relational);

            var optionsBuilder = new DbContextOptionsBuilder<RelationalContext>();
            optionsBuilder.UseSqlServer(_connectionString,
                b => b.MigrationsAssembly("Infrastructure"));
            _db = new RelationalContext(optionsBuilder.Options);
        }

        #region Setup Helper Methods

        private Card CreateTestCard()
        {
            var testCard = new Card
            {
                Name = "TestCard",
                Description = "A test card for unit testing.",
                Attack = 10,
                Defence = 5,
                Cost = 2,
                ImagePath = "images/testcard.png"
            };
            _db.Cards.Add(testCard);
            _db.SaveChanges();

            return testCard;
        }
        private Comment CreateTestComment()
        {
            var testUser = CreateTestUser();

            var testDeck = CreateTestDeck();

            var testComment = new Comment
            {
                DeckId = testDeck.Id,
                Text = "TestComment",
                CreatedAt = DateTime.UtcNow,
                UserId = testUser.Id
            };
            _db.Comments.Add(testComment);
            _db.SaveChanges();

            return testComment;
        }

        private DeckCard CreateTestDeckCard()
        {
            var testDeck = CreateTestDeck();
            var testCard = CreateTestCard();

            var testDeckCard = new DeckCard
            {
                DeckId = testDeck.Id,
                CardId = testCard.Id
            };
            _db.DeckCards.Add(testDeckCard);
            _db.SaveChanges();

            return testDeckCard;
        }

        private Deck CreateTestDeck()
        {
            var userId = CreateTestUser();

            var testDeck = new Deck
            {
                UserId = userId.Id,
                Name = "TestDeck",
                IsPublic = false
            };
            _db.Decks.Add(testDeck);
            _db.SaveChanges();

            return testDeck;
        }

        private Enemy CreateTestEnemy()
        {
            var testEnemy = new Enemy
            {
                Name = "TestEnemy",
                Health = 100,
                ImagePath = "images/testenemy.png"
            };
            _db.Enemies.Add(testEnemy);
            _db.SaveChanges();

            return testEnemy;
        }

        private Fight CreateTestFight()
        {
            var testUser = CreateTestUser();
            var testEnemy = CreateTestEnemy();

            var testFight = new Fight
            {
                UserId = testUser.Id,
                EnemyId = testEnemy.Id
            };
            _db.Fights.Add(testFight);
            _db.SaveChanges();

            return testFight;
        }

        private GameAction CreateTestGameAction()
        {
            var testFight = CreateTestFight();

            var testGameAction = new GameAction
            {
                FightId = testFight.Id,
                Type = "TEST",
                Value = 10
            };
            _db.GameActions.Add(testGameAction);
            _db.SaveChanges();
            
            return testGameAction;
        }
        private User CreateTestUser()
        {
            var testUser = new User
            {
                Username = "TestUser",
                PasswordHash = "TestHash", // Replace with a valid hash
                Role = "TestRole"
            };
            _db.Users.Add(testUser);
            _db.SaveChanges();

            return testUser;
        }

        #endregion

        #region Card Trigger Tests

        [Fact]
        public void Card_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testCard = CreateTestCard();

            // Allow triggers to execute
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Cards" && a.OperationType == "INSERT")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Cards", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains("TestCard", auditLogEntry.NewValues);
        }

        [Fact]
        public void Card_Update_ShouldLogToAuditLog()
        {
            // Arrange
            var testCard = CreateTestCard();

            // Act
            _testCard = _db.Cards.FirstOrDefault(c => c.Name == "TestCard");
            Assert.NotNull(_testCard); // Ensure the test card exists

            _testCard.Attack = 20; // Update a property
            _db.Cards.Update(_testCard);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the update operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Cards" && a.OperationType == "UPDATE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Cards", auditLogEntry.TableName);
            Assert.Equal("UPDATE", auditLogEntry.OperationType);
            Assert.Contains("TestCard", auditLogEntry.NewValues);
            Assert.Contains("\"Attack\":20", auditLogEntry.NewValues);
        }

        [Fact]
        public void Card_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testCard = CreateTestCard();

            // Act
            _testCard = _db.Cards.FirstOrDefault(c => c.Name == testCard.Name);
            Assert.NotNull(_testCard); // Ensure the test card exists

            _db.Cards.Remove(_testCard);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Cards" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Cards", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains("TestCard", auditLogEntry.OldValues);
        }

        #endregion

        //#region Comment Trigger Tests

        //[Fact]
        //public void Comment_Insert_ShouldLogToAuditLog()
        //{
        //    Arrange
        //   var testComment = CreateTestComment();

        //    Allow triggers to execute
        //    Task.Delay(1000).Wait();

        //    Query the AuditLog to ensure the insert operation was logged
        //    var auditLogEntry = _db.Set<AuditLog>()
        //                           .Where(a => a.TableName == "Comments" && a.OperationType == "INSERT")
        //                           .OrderByDescending(a => a.ChangeDateTime)
        //                           .FirstOrDefault();

        //    Assert
        //    Assert.NotNull(auditLogEntry);
        //    Assert.Equal("Comments", auditLogEntry.TableName);
        //    Assert.Equal("INSERT", auditLogEntry.OperationType);
        //    Assert.Contains("TestComment", auditLogEntry.NewValues);

        //    Cleanup
        //   var commentsToRemove = _db.Comments.Where(c => c.Text == testComment.Text).ToList();
        //    _db.Comments.RemoveRange(commentsToRemove);

        //    var auditLogs = _db.Set<AuditLog>()
        //                       .Where(a => a.NewValues.Contains(testComment.Text) || a.OldValues.Contains(testComment.Text));
        //    _db.Set<AuditLog>().RemoveRange(auditLogs);
        //    _db.SaveChanges();
        //}

        //[Fact]
        //public void Comment_Update_ShouldLogToAuditLog()
        //{
        //    Arrange
        //   var testComment = CreateTestComment();

        //    _testComment = _db.Comments.FirstOrDefault(c => c.Text == "TestComment");
        //    Assert.NotNull(_testComment);

        //    Act
        //    testComment.Text = "UpdatedComment";
        //    _db.Comments.Update(testComment);
        //    _db.SaveChanges();

        //    Task.Delay(1000).Wait();

        //    Query the AuditLog to ensure the update operation was logged
        //    var auditLogEntry = _db.Set<AuditLog>()
        //                           .Where(a => a.TableName == "Comments" && a.OperationType == "UPDATE")
        //                           .OrderByDescending(a => a.ChangeDateTime)
        //                           .FirstOrDefault();

        //    Assert
        //    Assert.NotNull(auditLogEntry);
        //    Assert.Equal("Comments", auditLogEntry.TableName);
        //    Assert.Equal("UPDATE", auditLogEntry.OperationType);
        //    Assert.Contains("\"Text\":\"UpdatedComment\"", auditLogEntry.NewValues);

        //    Cleanup
        //   var commentsToRemove = _db.Comments.Where(c => c.Text == testComment.Text).ToList();
        //    _db.Comments.RemoveRange(commentsToRemove);

        //    var auditLogs = _db.Set<AuditLog>()
        //                       .Where(a => a.NewValues.Contains(testComment.Text) || a.OldValues.Contains(testComment.Text));
        //    _db.Set<AuditLog>().RemoveRange(auditLogs);
        //    _db.SaveChanges();
        //}

        //[Fact]
        //public void Comment_Delete_ShouldLogToAuditLog()
        //{
        //    Arrange
        //   var testComment = CreateTestComment();

        //    _testComment = _db.Comments.FirstOrDefault(c => c.Text == "TestComment");
        //    Assert.NotNull(_testComment);

        //    Act
        //    _db.Comments.Remove(testComment);
        //    _db.SaveChanges();

        //    Task.Delay(1000).Wait();

        //    Query the AuditLog to ensure the delete operation was logged
        //    var auditLogEntry = _db.Set<AuditLog>()
        //                           .Where(a => a.TableName == "Comments" && a.OperationType == "DELETE")
        //                           .OrderByDescending(a => a.ChangeDateTime)
        //                           .FirstOrDefault();

        //    Assert
        //    Assert.NotNull(auditLogEntry);
        //    Assert.Equal("Comments", auditLogEntry.TableName);
        //    Assert.Equal("DELETE", auditLogEntry.OperationType);
        //    Assert.Contains("TestComment", auditLogEntry.OldValues);

        //    Cleanup
        //   var auditLogs = _db.Set<AuditLog>()
        //                      .Where(a => a.NewValues.Contains(testComment.Text) || a.OldValues.Contains(testComment.Text));
        //    _db.Set<AuditLog>().RemoveRange(auditLogs);
        //    _db.SaveChanges();
        //}

        //#endregion

        #region DeckCard Trigger Tests

        [Fact]
        public void DeckCard_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testDeckCard = CreateTestDeckCard();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "DeckCards" && a.OperationType == "INSERT")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("DeckCards", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains(testDeckCard.DeckId.ToString(), auditLogEntry.NewValues);
        }

        [Fact]
        public void DeckCard_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testDeckCard = CreateTestDeckCard();

            _testDeckCard = _db.DeckCards.FirstOrDefault(dc => dc.Deck.Name == "TestDeck");
            Assert.NotNull(_testDeckCard);

            var assertDeckId = _testDeckCard.DeckId.ToString();

            // Act
            _db.DeckCards.Remove(_testDeckCard);
            _db.SaveChanges();

            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "DeckCards" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("DeckCards", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains(assertDeckId, auditLogEntry.OldValues);
        }

        #endregion

        #region Deck Trigger Tests

        [Fact]
        public void Deck_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testDeck = CreateTestDeck();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Decks" && a.OperationType == "INSERT")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Decks", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains("TestDeck", auditLogEntry.NewValues);
        }

        [Fact]
        public void Deck_Update_ShouldLogToAuditLog()
        {
            // Arrange
            var testDeck = CreateTestDeck();

            _testDeck = _db.Decks.FirstOrDefault(d => d.Name == "TestDeck");
            Assert.NotNull(_testDeck);

            // Act
            _testDeck.IsPublic = true;
            _db.Decks.Update(_testDeck);
            _db.SaveChanges();

            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the update operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Decks" && a.OperationType == "UPDATE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Decks", auditLogEntry.TableName);
            Assert.Equal("UPDATE", auditLogEntry.OperationType);
            Assert.Contains("TestDeck", auditLogEntry.NewValues);
            Assert.Contains("\"IsPublic\":true", auditLogEntry.NewValues);
        }

        [Fact]
        public void Deck_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testDeck = CreateTestDeck();

            _testDeck = _db.Decks.FirstOrDefault(d => d.Name == "TestDeck");
            Assert.NotNull(_testDeck);

            // Act
            _db.Decks.Remove(_testDeck);
            _db.SaveChanges();

            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Decks" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Decks", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains("TestDeck", auditLogEntry.OldValues);
        }

        #endregion

        #region Enemy Trigger Tests

        [Fact]
        public void Enemy_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testEnemy = CreateTestEnemy();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                          .Where(a => a.TableName == "Enemies" && a.OperationType == "INSERT")
                                          .OrderByDescending(a => a.ChangeDateTime)
                                          .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Enemies", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains("TestEnemy", auditLogEntry.NewValues);
        }

        [Fact]
        public void Enemy_Update_ShouldLogToAuditLog()
        {
            // Arrange
            var testEnemy = CreateTestEnemy();

            // Act
            _testEnemy = _db.Enemies.FirstOrDefault(e => e.Name == "TestEnemy");
            Assert.NotNull(_testEnemy); // Ensure the test enemy exists

            _testEnemy.Health = 150;
            _db.Enemies.Update(_testEnemy);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000);

            // Query the AuditLog to ensure the update operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Enemies" && a.OperationType == "UPDATE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Enemies", auditLogEntry.TableName);
            Assert.Equal("UPDATE", auditLogEntry.OperationType);
            Assert.Contains("TestEnemy", auditLogEntry.NewValues);
            Assert.Contains("\"Health\":150", auditLogEntry.NewValues);
        }

        [Fact]
        public void Enemy_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testEnemy = CreateTestEnemy();

            // Act
            _testEnemy = _db.Enemies.FirstOrDefault(e => e.Name == "TestEnemy");
            Assert.NotNull(_testEnemy); // Ensure the test enemy exists

            _db.Enemies.Remove(_testEnemy);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000);

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Enemies" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Enemies", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains("TestEnemy", auditLogEntry.OldValues);
        }

        #endregion

        #region Fight Trigger Tests

        [Fact]
        public void Fight_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testFight = CreateTestFight();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Fights" && a.OperationType == "INSERT")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Fights", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains(testFight.EnemyId.ToString(), auditLogEntry.NewValues);
        }

        [Fact]
        public void Fight_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testFight = CreateTestFight();

            // Act
            _testFight = _db.Fights.FirstOrDefault(f => f.User.Username == "TestUser");
            Assert.NotNull(_testFight); // Ensure the test fight exists

            var assertEnemyId = _testFight.EnemyId.ToString();

            _db.Fights.Remove(_testFight);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Fights" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Fights", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains(assertEnemyId, auditLogEntry.OldValues);
        }

        #endregion

        #region GameAction Trigger Tests

        [Fact]
        public void GameAction_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testGameAction = CreateTestGameAction();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "GameActions" && a.OperationType == "INSERT")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("GameActions", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains(testGameAction.FightId.ToString(), auditLogEntry.NewValues);
        }

        [Fact]
        public void GameAction_Update_ShouldLogToAuditLog()
        {
            // Arrange
            var testGameAction = CreateTestGameAction();

            // Act
            _testGameAction = _db.GameActions.FirstOrDefault(ga => ga.FightId == testGameAction.FightId);
            Assert.NotNull(_testGameAction); // Ensure the test game action exists

            _testGameAction.Value = 20;
            _db.GameActions.Update(_testGameAction);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the update operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "GameActions" && a.OperationType == "UPDATE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("GameActions", auditLogEntry.TableName);
            Assert.Equal("UPDATE", auditLogEntry.OperationType);
            Assert.Contains(testGameAction.FightId.ToString(), auditLogEntry.NewValues);
            Assert.Contains("\"Value\":20", auditLogEntry.NewValues);
        }

        [Fact]
        public void GameAction_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testGameAction = CreateTestGameAction();

            // Act
            _testGameAction = _db.GameActions.FirstOrDefault(ga => ga.FightId == testGameAction.FightId);
            Assert.NotNull(_testGameAction); // Ensure the test game action exists

            var assertFightId = _testGameAction.FightId.ToString();

            _db.GameActions.Remove(_testGameAction);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "GameActions" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("GameActions", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains(assertFightId, auditLogEntry.OldValues);
        }

        #endregion

        #region User Trigger Tests

        [Fact]
        public void User_Insert_ShouldLogToAuditLog()
        {
            // Arrange
            var testUser = CreateTestUser();

            // Query the AuditLog to ensure the insert operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Users" && a.OperationType == "INSERT")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Users", auditLogEntry.TableName);
            Assert.Equal("INSERT", auditLogEntry.OperationType);
            Assert.Contains("TestUser", auditLogEntry.NewValues);
        }

        [Fact]
        public void User_Update_ShouldLogToAuditLog()
        {
            // Arrange
            var testUser = CreateTestUser();

            // Act
            _testUser = _db.Users.FirstOrDefault(u => u.Username == "TestUser");
            Assert.NotNull(_testUser); // Ensure the test user exists

            _testUser.Role = "UpdatedRole";
            _db.Users.Update(_testUser);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the update operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Users" && a.OperationType == "UPDATE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Users", auditLogEntry.TableName);
            Assert.Equal("UPDATE", auditLogEntry.OperationType);
            Assert.Contains("TestUser", auditLogEntry.NewValues);
            Assert.Contains("\"Role\":\"UpdatedRole\"", auditLogEntry.NewValues);
        }

        [Fact]
        public void User_Delete_ShouldLogToAuditLog()
        {
            // Arrange
            var testUser = CreateTestUser();

            // Act
            _testUser = _db.Users.FirstOrDefault(u => u.Username == "TestUser");
            Assert.NotNull(_testUser); // Ensure the test user exists

            _db.Users.Remove(_testUser);
            _db.SaveChanges();

            // Adjust this based on trigger execution time
            Task.Delay(1000).Wait();

            // Query the AuditLog to ensure the delete operation was logged
            var auditLogEntry = _db.Set<AuditLog>()
                                   .Where(a => a.TableName == "Users" && a.OperationType == "DELETE")
                                   .OrderByDescending(a => a.ChangeDateTime)
                                   .FirstOrDefault();

            // Assert
            Assert.NotNull(auditLogEntry);
            Assert.Equal("Users", auditLogEntry.TableName);
            Assert.Equal("DELETE", auditLogEntry.OperationType);
            Assert.Contains("TestUser", auditLogEntry.OldValues);
        }

        #endregion


        public void Dispose()
        {
            // Remove dependent entities first to avoid foreign key issues
            _db.GameActions.RemoveRange(_db.GameActions.Where(ga => ga.Type == "ATTACK" && ga.Value == 10));
            _db.Fights.RemoveRange(_db.Fights.Where(f => f.UserId != null && f.EnemyId != null));
            _db.DeckCards.RemoveRange(_db.DeckCards.ToList());
            _db.Comments.RemoveRange(_db.Comments.Where(c => c.Text == "TestComment"));
            _db.Cards.RemoveRange(_db.Cards.Where(c => c.Name == "TestCard"));
            _db.Decks.RemoveRange(_db.Decks.Where(d => d.Name == "TestDeck"));
            _db.Enemies.RemoveRange(_db.Enemies.Where(e => e.Name == "TestEnemy"));
            _db.Users.RemoveRange(_db.Users.Where(u => u.Username == "TestUser"));

            // Find and remove AuditLog entries related to the test objects
            var auditLogsToRemove = _db.Set<AuditLog>()
                                        .Where(a => a.NewValues.Contains("TestCard")
                                                 || a.NewValues.Contains("TestComment")
                                                 || a.NewValues.Contains("TestDeck")
                                                 || a.NewValues.Contains("TestEnemy")
                                                 || a.NewValues.Contains("TestUser")
                                                 || a.OldValues.Contains("TestCard")
                                                 || a.OldValues.Contains("TestComment")
                                                 || a.OldValues.Contains("TestDeck")
                                                 || a.OldValues.Contains("TestEnemy")
                                                 || a.OldValues.Contains("TestUser"))
                                        .ToList();
            _db.Set<AuditLog>().RemoveRange(auditLogsToRemove);
            _db.SaveChanges();
        }

    }

}
