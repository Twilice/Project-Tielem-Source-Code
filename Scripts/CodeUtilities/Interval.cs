using System;

public static class Interval
{
	/// <summary>
	/// Checks if target is inside the closed interval from / to.
	/// If any endpoint is null target is regarded as inside that endpoint.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	public static bool InInterval<T>(T target, T? from, T? to) where T : struct, IComparable<T>
	{
		return (from == null || from.Value.CompareTo(target) <= 0)
			   && (to == null || target.CompareTo(to.Value) <= 0);
	}

	/// <summary>
	/// Checks if target is inside the closed interval from / to.
	/// If any endpoint is null target is regarded as inside that endpoint.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <returns></returns>
	public static bool InInterval<T>(T target, T from, T to) where T : IComparable<T>
	{
		return (from == null || from.CompareTo(target) <= 0)
			   && (to == null || target.CompareTo(to) <= 0);
	}

	/// <summary>
	/// Checks if target is inside the interval from / to.
	/// If any endpoint is null target is regarded as inside that endpoint.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="intervalType"></param>
	/// <returns></returns>
	public static bool InInterval<T>(T target, T? from, T? to, IntervalType intervalType) where T : struct, IComparable<T>
	{
		switch(intervalType)
		{
			case IntervalType.Closed:
				return (from == null || from.Value.CompareTo(target) <= 0)
					   && (to == null || target.CompareTo(to.Value) <= 0);
			case IntervalType.LeftClosed:
				return (from == null || from.Value.CompareTo(target) <= 0)
					   && (to == null || target.CompareTo(to.Value) < 0);
			case IntervalType.RightClosed:
				return (from == null || from.Value.CompareTo(target) < 0)
					   && (to == null || target.CompareTo(to.Value) <= 0);
			case IntervalType.Open:
				return (from == null || from.Value.CompareTo(target) < 0)
					   && (to == null || target.CompareTo(to.Value) < 0);
			default:
				throw new InvalidOperationException();
		}
	}

	/// <summary>
	/// Checks if target is inside the interval from / to.
	/// If any endpoint is null target is regarded as inside that endpoint.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="target"></param>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="intervalType"></param>
	/// <returns></returns>
	public static bool InInterval<T>(T target, T from, T to, IntervalType intervalType) where T : IComparable<T>
	{
		switch(intervalType)
		{
			case IntervalType.Closed:
				return (from == null || from.CompareTo(target) <= 0)
					   && (to == null || target.CompareTo(to) <= 0);
			case IntervalType.LeftClosed:
				return (from == null || from.CompareTo(target) <= 0)
					   && (to == null || target.CompareTo(to) < 0);
			case IntervalType.RightClosed:
				return (from == null || from.CompareTo(target) < 0)
					   && (to == null || target.CompareTo(to) <= 0);
			case IntervalType.Open:
				return (from == null || from.CompareTo(target) < 0)
					   && (to == null || target.CompareTo(to) < 0);
			default:
				throw new InvalidOperationException();
		}
	}
	public enum IntervalType
	{
		Closed,
		LeftClosed,
		RightClosed,
		Open,
	}
}
