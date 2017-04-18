using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhomCombination : Combination {
	public enum Type
	{
		TRASH,
		STRAIGHT,
		RANK_SET
	};

	public PhomCombination() : base() {
	}

	public PhomCombination(List<Card> cards) : base(cards) {
	}

	public PhomCombination(string cardsString) : base(cardsString) {
	}

	public override object CreateInstance(string cardString) {
		return new PhomCard(cardString);
	}

	public PhomCard At(int index) {
		return (PhomCard)base.At(index);
	}

	public bool IsStraight() {
		if (numCards <= 2)
			return false;

		Sort (Order.ASC);

		if (At(numCards - 1).rank - At(0).rank != numCards - 1) {
			return false;
		}

		for (int i = 0; i < numCards - 1; i++) {
			if (At(i).suit != At(i + 1).suit) {
				return false;
			}
		}

		return true;
	}

	public bool IsRankSet() {
		if (numCards <= 2 || numCards > 4)
			return false;
		if (At (0).HasSameRank (At (numCards - 1))) {
			return true;
		}

		return false;
	}

	protected override void FindType() {
		Sort (Order.ASC);
		type = (int)Type.TRASH;
		if (IsRankSet ()) {
			type = (int)Type.RANK_SET;
		} else if (IsStraight ()) {
			type = (int)Type.STRAIGHT;
		}
	}

	public override string GetTypeName() {
		switch ((Type)type) {
		case Type.RANK_SET:
			return "Rank set";
		case Type.STRAIGHT:
			return "Straight";
		default:
			return "Trash";
		}
	}
}