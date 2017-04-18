using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhomHand : Hand {
	private CardSet takenCards = new CardSet ();
	private CardSet singleCards = new CardSet ();

	public PhomHand() : base() {
		combinations = new List<Combination>();
	}

	public PhomHand(List<Card> cards) : base(cards) {
		combinations = new List<Combination>();
	}

	public override object CreateInstance(string cardString) {
		return new PhomCard(cardString);
	}

	public override List<Combination> FindCombinations() {
		singleCards.Reset ();
		combinations.Clear ();

		Sort (Order.ASC);

		int i = 0;
		while (true) {
			if (i >= numCards - 2) {
				for (int j = i; j < numCards; j++) {
					singleCards.AddCard (At (j));
				}
				break;
			}

			PhomCombination combination = new PhomCombination (SubSetList (i, 3));
			if (combination.IsRankSet ()) {
				PhomCombination combination2 = null;
				if (i + 4 < numCards) {
					combination2 = new PhomCombination (SubSetList (i, 4));
				}
				if (combination2 != null && combination2.IsRankSet ()) {
					i += 4;
					combinations.Add (combination2);
				} else {
					combinations.Add (combination);
					i += 3;
				}
			} else if (combination.IsStraight()) {
				int j = 4;
				PhomCombination combination2 = null;
				while (i + j < numCards) {
					combination2 = new PhomCombination (SubSetList (i, j));
					if (combination2.IsStraight ())
						combination = combination2;
					else 
						break;
					j++;
				}
				combinations.Add (combination);
				i += j - 1;
			} else {
				singleCards.AddCard (At (i));
				i++;
			}
		}

		return combinations;
	}

	void ClassifyCards(ref CardSet oneSetCards, ref CardSet twoSetsCards) {
		for (int i = 0; i < cards.Count; i++) {
			int numSameRank = 0;
			for (int j = 0; j < cards.Count; j++) {
				if (i != j && cards [i].HasSameRank (cards [j])) {
					numSameRank++;
					if (numSameRank >= 3)
						break;
				}
			}

			bool isInStraight = false;
			bool hasLowerCard = false;
			bool hasLowerCard2 = false;
			bool hasHigherCard = false;
			bool hasHigherCard2 = false;

			for (int j = 0; j < cards.Count; j++) {
				if (i != j && cards [i].HasSameSuit (cards [j])) {
					if (cards [i].GetRank () == cards [j].GetRank () + 1) {
						hasLowerCard = true;
					} else if (cards [i].GetRank () == cards [j].GetRank () - 1) {
						hasHigherCard = true;
					} else if (cards[i].GetRank () == cards [j].GetRank () - 2) {
						hasHigherCard2 = true;
					} else if (cards[i].GetRank () == cards [j].GetRank () + 2) {
						hasLowerCard2 = true;
					}
				}
				isInStraight = ((hasLowerCard && hasHigherCard) || (hasLowerCard && hasLowerCard2) || (hasHigherCard && hasHigherCard2));
				if (isInStraight)
					break;
			}

			if (numSameRank > 1 && isInStraight) {
				twoSetsCards.AddCard (cards [i]);
			} else if (numSameRank > 1 || isInStraight) {
				oneSetCards.AddCard (cards [i]);
			}
		}

//		Debug.Log ("Check:" + ToCardString ());
//		Debug.Log ("One:" + oneSetCards.ToCardString ());
//		Debug.Log ("Two:" + twoSetsCards.ToCardString () + " " + twoSetsCards.Length());
	}

	bool BestSubHand(CardSet subCardSet, int exceptIndex, ref int bestScore) {
		PhomHand hand = new PhomHand (SubSetList (0, numCards));

		if (exceptIndex == -1) {
			foreach (Card c in subCardSet.cards) {
				hand.RemoveCard (c);
			}
		} else {
			for (int n = 0; n < subCardSet.Length (); n++) {
				if (n != exceptIndex) {
					hand.RemoveCard (subCardSet.At (n));
				}
			}
		}

		return CheckBestScore (hand, ref bestScore, true);
	}

	bool BestSubHandWithNoCardInTwoSet(CardSet subCardSet, ref int bestScore) {
		PhomHand hand = new PhomHand (SubSetList (0, numCards));
		foreach (Card c in subCardSet.cards) {
			hand.RemoveCard (c);
		}

		hand.FindCombinations ();
		return CheckBestScore (hand, ref bestScore, false);

//		int tmpScore = hand.ComputeScore (false);
//		if (tmpScore < bestScore) {
//			bestScore = tmpScore;
//		}
//
//		return bestScore == 0;
	}

	bool CheckBestScore(PhomHand hand, ref int bestScore, bool shouldCheckBest) {
		int tmpScore = shouldCheckBest? hand.Best () : hand.ComputeScore(false);
		if (tmpScore <= bestScore) {
			bestScore = tmpScore;
		}

		return bestScore == 0;
	}
		
	public int Best() {
		Sort (Order.ASC);

		CardSet oneSetCards = new CardSet ();
		CardSet twoSetsCards = new CardSet ();

		ClassifyCards (ref oneSetCards, ref twoSetsCards);

		int bestScore = 1000;

		// There is at least one card which is in two sets.
		if (twoSetsCards.Length () > 0) {
			int tmpScore = 0;
			for (int i = 0; i < twoSetsCards.Length (); i++) {
//				Debug.Log ("Rank set:" + ToCardString ());
				// First check same rank set.
				CardSet rankSet = FindRankSetCombination ((PhomCard)twoSetsCards.At (i));

				List<int> overlappedIndice = new List<int> ();
				if (rankSet.Length () == 4) {
					for (int j = 0; j < twoSetsCards.Length (); j++) {
						if (i != j && rankSet.Contains (twoSetsCards.At (j))) {
							overlappedIndice.Add (j);
							break;
						}
					}
				}

				// Two cards in the same rank set. Quad set.
				if (overlappedIndice.Count > 0) {
					for (int m = 0; m < overlappedIndice.Count; m++) {
						// Keep all in the rank set.
						if (BestSubHand (rankSet, -1, ref bestScore))
							return 0;

						// Remove the overlapped out of rank set.
						if (BestSubHand (rankSet, overlappedIndice [m], ref bestScore))
							return 0;
					}
				} else {
					if (BestSubHand (rankSet, -1, ref bestScore))
						return 0;
				}


				// Check straight set.
//				Debug.Log ("Straight set:" + ToCardString ());
				CardSet straightSet = FindStraightCombination ((PhomCard)twoSetsCards.At (i));
//				Debug.Log ("Straight set2:" + straightSet.ToCardString ());
				int mainIndex = straightSet.cards.IndexOf (twoSetsCards.At (i));
				overlappedIndice.Clear ();

				for (int j = 0; j < twoSetsCards.Length (); j++) {
					if (i != j && straightSet.Contains (twoSetsCards.At (j))) {
						overlappedIndice.Add (j);
						break;
					}
				}

				// Two cards in the same straight.
				if (overlappedIndice.Count > 0) {
					for (int m = 0; m < overlappedIndice.Count; m++) {
						// Keep the overlapped card in the straight hand <=> remove it from the remaining.
						if (BestSubHand (straightSet, -1, ref bestScore))
							return 0;

						// Remove the overlapped card in the set <=> not remove it from the remaining.
						if ((int)Mathf.Abs (mainIndex - overlappedIndice [m]) > 2) {
							PhomHand straightHand = new PhomHand (SubSetList (0, numCards));
							if (mainIndex < overlappedIndice [m]) {
								for (int k = overlappedIndice [m]; k < straightSet.Length (); k++) {
									straightHand.RemoveCard (straightSet.At (k));
								}
							} else {
								for (int k = 0; k <= overlappedIndice [m]; k++) {
									straightHand.RemoveCard (straightSet.At (k));
								}
							}

							if (CheckBestScore (straightHand, ref bestScore, true))
								return 0;
						}
					}
				}
				// 
				else {
					if (BestSubHand (straightSet, -1, ref bestScore))
						return 0;
				}
			}
		} 
		// No card in two sets.
		else if (oneSetCards.Length () > 0) {
			int tmpScore = 0;
//			Debug.Log ("++++++++:" + oneSetCards.ToCardString () + " " + oneSetCards.Length ());
			for (int i = 0; i < oneSetCards.Length (); i++) {
				// First check same rank set.
				CardSet rankSet = FindRankSetCombination ((PhomCard)oneSetCards.At (i));
//				Debug.Log ("++++++++2:" + rankSet.ToCardString ());
				if (rankSet.Length () > 0 && BestSubHandWithNoCardInTwoSet (rankSet, ref bestScore))
					return 0;

				// Check straight set.
				CardSet straightSet = FindStraightCombination ((PhomCard)oneSetCards.At (i));
//				Debug.Log ("Set one: straight set:" + straightSet.ToCardString ());
				if (straightSet.Length () > 0 && BestSubHandWithNoCardInTwoSet (straightSet, ref bestScore))
					return 0;
			}
		} else {
//			Debug.Log ("no set found:" + ToCardString ());
			singleCards.cards.AddRange (cards);
			if (CheckBestScore (this, ref bestScore, false))
				return 0;
		}

//		Debug.Log ("bestScore:" + bestScore);
		return bestScore;
	}


	public CardSet FindRankSetCombination(PhomCard card) {
		CardSet combination = new CardSet ();
		foreach (PhomCard c in cards) {
			if (card.HasSameRank (c)) {
				combination.AddCard (c);
			}
		}

		if (combination.Length () <= 2) {
			combination.Reset ();
		} else {
			combination.Sort (Order.ASC);
		}
		return combination;
	}

	public CardSet FindStraightCombination(PhomCard card) {
		CardSet combination = new CardSet ();
		int topRank = card.GetRank ();
		int bottomRank = topRank;
		int index = cards.IndexOf (card);
		for (int i = index - 1; i >= 0; i--) {
			if (card.HasSameSuit (cards [i]) && cards [i].GetRank () == bottomRank - 1) {
				combination.AddCard (cards [i]);
				bottomRank--;
			}
		}

		combination.AddCard (card);

		for (int i = index + 1; i < cards.Count; i++) {
			if (card.HasSameSuit (cards [i]) && cards [i].GetRank () == topRank + 1) {
				combination.AddCard (cards [i]);
				topRank++;
			}
		}

		if (combination.Length () <= 2) {
			combination.Reset ();
		} else {
			combination.Sort (Order.ASC);
		}
		return combination;
	}

	public override void Arrange() {
		List<Card> tmp = new List<Card>(cards);
		Reset ();
		foreach (PhomCombination c in combinations) {
			cards.AddRange (c.cards);
		}
		cards.AddRange (singleCards.cards);
		numCards = cards.Count;
		Debug.Log (ToCardString ());
	}

	public void TakeCard(PhomCard card) {
		
	}

	public bool CanTakeCard(PhomCard card) {
		return false;
	}

	public bool CanDropCard(PhomCard card) {
		return true;
	}

	public void DropCard(PhomCard card) {
	}

	public int ComputeScore(bool auto = true) {
		int score = 0;

		if (auto) {
			return Best ();
		}

		// No single cards => U.
		if (singleCards.Length() == 0) {
			return 0;
		} 
		// No combinations => Mom.
		else if (singleCards.Length() == Length () && Length() >= 9) {
			return 1000;
		}

		Debug.Log ("Compute:" + ToCardString () + " ====" + singleCards.ToCardString());

		foreach (PhomCard card in singleCards.cards) {
			score += card.GetRank () + 1;
		}

		return score;
	}
}



//public int Best() {
//	Sort (Order.ASC);
//
//	CardSet oneSetCards = new CardSet ();
//	CardSet twoSetsCards = new CardSet ();
//
//	ClassifyCards (ref oneSetCards, ref twoSetsCards);
//
//	int bestScore = 1000;
//
//	// There is at least one card which is in two sets.
//	if (twoSetsCards.Length () > 0) {
//		int tmpScore = 0;
//		for (int i = 0; i < twoSetsCards.Length (); i++) {
//			Debug.Log ("Rank set:" + ToCardString ());
//			// First check same rank set.
//			CardSet rankSet = FindRankSetCombination ((PhomCard)twoSetsCards.At (i));
//
//			List<int> overlappedIndice = new List<int> ();
//			if (rankSet.Length () == 4) {
//				for (int j = 0; j < twoSetsCards.Length (); j++) {
//					if (i != j && rankSet.Contains (twoSetsCards.At (j))) {
//						overlappedIndice.Add (j);
//						break;
//					}
//				}
//			}
//
//			// Two cards in the same rank set. Quad set.
//			if (overlappedIndice.Count > 0) {
//				for (int m = 0; m < overlappedIndice.Count; m++) {
//					// Keep all in the rank set.
//					//						PhomHand hand = new PhomHand (SubSetList (0, numCards));
//					//						foreach (Card c in rankSet.cards) {
//					//							hand.RemoveCard (c);
//					//						}
//					//						tmpScore = hand.Best ();
//					//						if (tmpScore < bestScore) {
//					//							bestScore = tmpScore;
//					//						}
//					//
//					//						if (tmpScore == 0) {
//					//							return 0;
//					//						}
//
//					if (BestSubHand (rankSet, -1, ref bestScore))
//						return 0;
//
//					// Remove the overlapped out of rank set.
//					//						hand = new PhomHand (SubSetList (0, numCards));
//					//						for (int n = 0; n < rankSet.Length (); n++) {
//					//							if (n != overlappedIndice [m]) {
//					//								hand.RemoveCard (rankSet.At (n));
//					//							}
//					//						}
//					//
//					//						tmpScore = hand.Best ();
//					//						if (tmpScore < bestScore) {
//					//							bestScore = tmpScore;
//					//						}
//					//
//					//						if (tmpScore == 0) {
//					//							return 0;
//					//						}
//
//					if (BestSubHand (rankSet, overlappedIndice [m], ref bestScore))
//						return 0;
//				}
//			} else {
//				//					PhomHand hand = new PhomHand (SubSetList (0, numCards));
//				//					foreach (Card c in rankSet.cards) {
//				//						hand.RemoveCard (c);
//				//					}
//				//
//				//					tmpScore = hand.Best ();
//				//					if (tmpScore < bestScore) {
//				//						bestScore = tmpScore;
//				//					}
//				//
//				//					if (tmpScore == 0) {
//				//						return 0;
//				//					}
//
//				if (BestSubHand (rankSet, -1, ref bestScore))
//					return 0;
//			}
//
//
//			// Check straight set.
//			Debug.Log ("Straight set:" + ToCardString ());
//			CardSet straightSet = FindStraightCombination ((PhomCard)twoSetsCards.At (i));
//			Debug.Log ("Straight set2:" + straightSet.ToCardString ());
//			int mainIndex = straightSet.cards.IndexOf (twoSetsCards.At (i));
//			overlappedIndice.Clear ();
//
//			for (int j = 0; j < twoSetsCards.Length (); j++) {
//				if (i != j && straightSet.Contains (twoSetsCards.At (j))) {
//					overlappedIndice.Add (j);
//					break;
//				}
//			}
//
//			// Two cards in the same straight.
//			if (overlappedIndice.Count > 0) {
//				for (int m = 0; m < overlappedIndice.Count; m++) {
//					// Keep the overlapped card in the straight hand <=> remove it from the remaining.
//					//						PhomHand straightHand = new PhomHand (SubSetList (0, numCards));
//					//						foreach (Card c in straightSet.cards) {
//					//							straightHand.RemoveCard (c);
//					//						}
//					//						tmpScore = straightHand.Best ();
//					//						if (tmpScore < bestScore) {
//					//							bestScore = tmpScore;
//					//						}
//					//
//					//						if (tmpScore == 0) {
//					//							return 0;
//					//						}
//					if (BestSubHand (straightSet, -1, ref bestScore))
//						return 0;
//
//					// Remove the overlapped card in the set <=> not remove it from the remaining.
//					if ((int)Mathf.Abs (mainIndex - overlappedIndice [m]) > 2) {
//						PhomHand straightHand = new PhomHand (SubSetList (0, numCards));
//						if (mainIndex < overlappedIndice [m]) {
//							for (int k = overlappedIndice [m]; k < straightSet.Length (); k++) {
//								straightHand.RemoveCard (straightSet.At (k));
//							}
//						} else {
//							for (int k = 0; k <= overlappedIndice [m]; k++) {
//								straightHand.RemoveCard (straightSet.At (k));
//							}
//						}
//
//						//							tmpScore = straightHand.Best ();
//						//							if (tmpScore < bestScore) {
//						//								bestScore = tmpScore;
//						//							}
//						//
//						//							if (tmpScore == 0) {
//						//								return 0;
//						//							}
//
//						if (CheckBestScore (straightHand, ref bestScore, true))
//							return 0;
//					}
//				}
//			}
//			// 
//			else {
//				//					PhomHand straightHand = new PhomHand (SubSetList (0, numCards));
//				//					foreach (Card c in straightSet.cards) {
//				//						straightHand.RemoveCard (c);
//				//					}
//				//					tmpScore = straightHand.Best ();
//				//					if (tmpScore < bestScore) {
//				//						bestScore = tmpScore;
//				//					}
//				//
//				//					if (tmpScore == 0) {
//				//						return 0;
//				//					}
//				if (BestSubHand (straightSet, -1, ref bestScore))
//					return 0;
//			}
//		}
//	} 
//	// No card in two sets.
//	else if (oneSetCards.Length () > 0) {
//		int tmpScore = 0;
//		Debug.Log ("++++++++:" + oneSetCards.ToCardString () + " " + oneSetCards.Length ());
//		for (int i = 0; i < oneSetCards.Length (); i++) {
//			// First check same rank set.
//			CardSet rankSet = FindRankSetCombination ((PhomCard)oneSetCards.At (i));
//			Debug.Log ("++++++++2:" + rankSet.ToCardString ());
//			if (rankSet.Length () > 0) {
//				//					PhomHand hand = new PhomHand (SubSetList (0, numCards));
//				//					foreach (Card c in rankSet.cards) {
//				//						hand.RemoveCard (c);
//				//					}
//				//
//				//					hand.FindCombinations ();
//				//					tmpScore = hand.ComputeScore (false);
//				//					if (tmpScore < bestScore) {
//				//						bestScore = tmpScore;
//				//					}
//				//
//				//					if (tmpScore == 0) {
//				//						return 0;
//				//					}
//				if (BestSubHandWithNoCardInTwoSet (rankSet, ref bestScore))
//					return 0;
//			}
//
//			// Check straight set.
//			CardSet straightSet = FindStraightCombination ((PhomCard)oneSetCards.At (i));
//			Debug.Log ("Set one: straight set:" + straightSet.ToCardString ());
//			if (straightSet.Length () > 0) {
//				//					PhomHand hand = new PhomHand (SubSetList (0, numCards));
//				//					foreach (Card c in straightSet.cards) {
//				//						hand.RemoveCard (c);
//				//					}
//				//					Debug.Log ("Set one: straight set2:" + hand.ToCardString ());
//				//					hand.FindCombinations ();
//				//					tmpScore = hand.ComputeScore (false);
//				//					if (tmpScore < bestScore) {
//				//						bestScore = tmpScore;
//				//					}
//				//
//				//					if (tmpScore == 0) {
//				//						return 0;
//				//					}
//				if (BestSubHandWithNoCardInTwoSet (straightSet, ref bestScore))
//					return 0;
//			}
//		}
//	} else {
//		Debug.Log ("no set found:" + ToCardString ());
//		singleCards.cards.AddRange (cards);
//		if (CheckBestScore (this, ref bestScore, false))
//			return 0;
//
//		//			int tmpScore = ComputeScore (false);
//		//			if (tmpScore < bestScore) {
//		//				bestScore = tmpScore;
//		//			}
//		//
//		//			if (tmpScore == 0) {
//		//				return 0;
//		//			}
//	}
//
//	Debug.Log ("bestScore:" + bestScore);
//	return bestScore;
//}