﻿/// <summary>
/// Comparer for comparing two keys, handling equality as being greater
/// Use this Comparer e.g. with SortedLists or SortedDictionaries, that don't allow duplicate keys
/// </summary>
/// <typeparam name="TKey"></typeparam>
public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable {
    #region IComparer<TKey> Members

    public int Compare(TKey x, TKey y) {
        var result = x.CompareTo(y);

        if (result == 0)
            return 1; // Handle equality as being greater. Note: this will break Remove(key) or

        // IndexOfKey(key) since the comparer never returns 0 to signal key equality
        return result;
    }

    #endregion
}