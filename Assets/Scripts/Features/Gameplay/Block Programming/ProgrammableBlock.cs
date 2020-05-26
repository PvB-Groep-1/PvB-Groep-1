﻿using CM.Events;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a code block.
/// </summary>
public class ProgrammableBlock : MonoBehaviour, IPointerDownHandler
{
	/// <summary>
	/// The current ProgrammableBlock attached to the mouse.
	/// </summary>
	public static ProgrammableBlock CurrentlyMovingProgrammableBlock { get; private set; }

	/// <summary>
	/// The snapping points of this block.
	/// </summary>
	public SnapPoints SnappingPoints => _snapPoints;

	/// <summary>
	/// The image of this block.
	/// </summary>
	public Image Image => _image;

	/// <summary>
	/// Returns true if this block is locked to the mouse.
	/// </summary>
	public bool IsLockedToMouse => _lockToMouse;

	private Vector3 _mouseSnapPoint;
	private bool _isSnapped = false;

	/// <summary>
	/// Represents 4 directions.
	/// </summary>
	public enum Direction
	{
		Left,
		Right,
		Up,
		Down
	}

	/// <summary>
	/// An event for when this block is released from another block.
	/// </summary>
	public event SimpleEvent OnReleaseSnap;

	[SerializeField]
	private SnapPoints _snapPoints;

	[SerializeField]
	private bool _lockToMouse = false;

	[SerializeField]
	private float _snapThreshold = 65f;

	[SerializeField]
	private Image _image;

	[SerializeField]
	private AudioClip _snapAudio;

	[SerializeField]
	private AudioClip _clickAudio;

	[Header("References")]

	

	private AudioSource _audioSource;

	private void Start()
    {
		_audioSource = GetComponent<AudioSource>();
    }

	private void Update()
	{
		if (!_lockToMouse)
			return;

		if (CurrentlyMovingProgrammableBlock != this)
			return;

		if (_isSnapped)
		{
			if (Vector3.Distance(_mouseSnapPoint, Input.mousePosition) > _snapThreshold)
				ReleaseSnap();

			return;
		}

		transform.position = Input.mousePosition;
		transform.position = new Vector3(
			Mathf.Clamp(transform.position.x, 410, 1520),
			Mathf.Clamp(transform.position.y, 430, 850),
			transform.position.z
		);
	}

	private bool CanSnapDirection(Direction direction1, Direction direction2)
	{
		// Left and Right Direction check.
		if (direction1 == Direction.Left && direction2 == Direction.Right)
			return true;

		// Right and Left Direction check.
		else if (direction1 == Direction.Right && direction2 == Direction.Left)
			return true;

		// Up and Down Direction check.
		else if (direction1 == Direction.Up && direction2 == Direction.Down)
			return true;

		// Down and Up Direction check.
		else if (direction1 == Direction.Down && direction2 == Direction.Up)
			return true;

		return false;
	}

	/// <summary>
	/// When you click on this object.
	/// </summary>
	/// <param name="eventData">The current data for the pointer.</param>
	public void OnPointerDown(PointerEventData eventData)
	{
		_audioSource.clip = _clickAudio;
		_audioSource.Play();

		if (tag == "StartNode")
			return;

		_lockToMouse = !_lockToMouse;
		_mouseSnapPoint = transform.position;

		if (_lockToMouse)
		{
			/*
				Move the transform to the end of the local transform list.
				This object will be drawn on top of other objects.
			*/
			transform.SetAsLastSibling();

			ReleaseSnap();

			CurrentlyMovingProgrammableBlock = this;
		}
		else
		{
			CurrentlyMovingProgrammableBlock = null;
		}
	}

	/// <summary>
	/// Snaps this block snapping point to another snapping point.
	/// </summary>
	/// <param name="referencePoint">The snapping point from this block.</param>
	/// <param name="snapPoint">The other snapping point that this block needs to attach to.</param>
	/// <returns>True if snapping is allowed for the current snapping points.</returns>
	public bool SnapTo(SnapPoint referencePoint, SnapPoint snapPoint)
	{
		if (!CanSnapDirection(_snapPoints.GetSnapPointData(referencePoint).direction, snapPoint.ProgrammableBlock.SnappingPoints.GetSnapPointData(snapPoint).direction))
			return false;

		Vector3 referenceDistance = referencePoint.transform.position - transform.position;

		transform.position = snapPoint.transform.position - referenceDistance;

		_mouseSnapPoint = Input.mousePosition;
		_isSnapped = true;

		_audioSource.clip = _snapAudio;
		_audioSource.Play();

		return true;
	}

	/// <summary>
	/// Releases this block from another block.
	/// </summary>
	public void ReleaseSnap()
	{
		_isSnapped = false;

		OnReleaseSnap?.Invoke();
	}

	/// <summary>
	/// Gets a connected ProgrammableBlock from a given Direction.
	/// </summary>
	/// <param name="direction">The direction of the connected block.</param>
	/// <returns>The connected block in a given Direction.</returns>
	public ProgrammableBlock GetConnectedProgrammableBlock(Direction direction)
	{
		ProgrammableBlock connectedProgrammableBlock = null;

		try
		{
			switch (direction)
			{
				case Direction.Left:
					connectedProgrammableBlock = SnappingPoints.left.snapPoint.ConnectedSnapPoint.ProgrammableBlock;
					break;
				case Direction.Right:
					connectedProgrammableBlock = SnappingPoints.right.snapPoint.ConnectedSnapPoint.ProgrammableBlock;
					break;
				case Direction.Up:
					connectedProgrammableBlock = SnappingPoints.up.snapPoint.ConnectedSnapPoint.ProgrammableBlock;
					break;
				case Direction.Down:
					connectedProgrammableBlock = SnappingPoints.down.snapPoint.ConnectedSnapPoint.ProgrammableBlock;
					break;
			}
		}
		catch
		{
			return null;
		}

		return connectedProgrammableBlock;
	}

	#region Editor Region

	private void OnValidate()
	{
		_snapPoints.SetDefaultDirections();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;

		if (_isSnapped && IsLockedToMouse)
			Gizmos.DrawWireSphere(_mouseSnapPoint, _snapThreshold);
	}

	#endregion

	/// <summary>
	/// Represents all snapping points for this block.
	/// </summary>
	[System.Serializable]
	public struct SnapPoints
	{
		/// <summary>
		/// The snapping point at the left side of this block.
		/// </summary>
		public SnapPointData left;

		/// <summary>
		/// The snapping point at the right side of this block.
		/// </summary>
		public SnapPointData right;

		/// <summary>
		/// The snapping point at the top side of this block.
		/// </summary>
		public SnapPointData up;

		/// <summary>
		/// The snapping point at the bottom side of this block.
		/// </summary>
		public SnapPointData down;

		/// <summary>
		/// Sets the correct directions.
		/// </summary>
		public void SetDefaultDirections()
		{
			left.direction = Direction.Left;
			right.direction = Direction.Right;
			up.direction = Direction.Up;
			down.direction = Direction.Down;
		}

		/// <summary>
		/// Gets the SnapPointData for a given SnapPoint.
		/// </summary>
		/// <param name="snapPoint">The given SnapPoint to get the SnapPointData from.</param>
		/// <returns>The SnapPointData for a given SnapPoint.</returns>
		public SnapPointData GetSnapPointData(SnapPoint snapPoint)
		{
			if (snapPoint == left.snapPoint)
				return left;

			else if (snapPoint == right.snapPoint)
				return right;

			else if (snapPoint == up.snapPoint)
				return up;

			else if(snapPoint == down.snapPoint)
				return down;

			return left;
		}

		/// <summary>
		/// Gets the SnapPointData for a given Direction.
		/// </summary>
		/// <param name="direction">The given Direction to get the SnapPointData from.</param>
		/// <returns>The SnapPointData for a given Direction.</returns>
		public SnapPointData GetSnapPointData(Direction direction)
		{
			switch (direction)
			{
				case Direction.Left:
					return left;
				case Direction.Right:
					return right;
				case Direction.Up:
					return up;
				case Direction.Down:
					return down;
			}

			return left;
		}
	}

	/// <summary>
	/// Represents the data for a snapping point.
	/// </summary>
	[System.Serializable]
	public struct SnapPointData
	{
		/// <summary>
		/// The direction for this snapping point.
		/// </summary>
		public Direction direction;

		/// <summary>
		/// The snapping point.
		/// </summary>
		public SnapPoint snapPoint;
	}
}