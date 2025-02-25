﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace MatchThreeEngine
{
	public sealed class Board : MonoBehaviour
	{
		[SerializeField] private TileTypeAsset[] tileTypes;

		[SerializeField] private Row[] rows;

		[SerializeField] private AudioClip matchSound;

		[SerializeField] private AudioSource audioSource;

		[SerializeField] private float tweenDuration;

		[SerializeField] private Transform swappingOverlay;

		[SerializeField] private bool ensureNoStartingMatches;

		[SerializeField] private RectTransform selectBorder;

		private readonly List<Tile> _selection = new List<Tile>();

		private bool _isSwapping;
		private bool _isMatching;
		private bool _isShuffling;

		public event Action<TileTypeAsset, int> OnMatch;
		public event Action<Tile> OnTileClick;

		public int ChoiseCount => _selection.Count;
		public TileTypeAsset[] TileTypes => tileTypes;

		public void FlipBooster(Transform particle)
        {
			_selection.Clear();

			var bestMove = TileDataMatrixUtility.FindBestMove(Matrix);

			if (bestMove != null)
			{
				var one = GetTile(bestMove.X1, bestMove.Y1);
				var two = GetTile(bestMove.X2, bestMove.Y2);

				particle.position = (one.transform.position + two.transform.position) / 2;

				Select(one);
				Select(two);
			}
		}

		public void DiscoBooster()
        {
			var typeId = _selection[0].Type.id;
			var count = 0;

			_selection.Clear();

			foreach (var row in rows)
				foreach (var tile in row.tiles)
					if(tile.Type.id == typeId)
                    {
						count++;
						tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];
                    }


			OnMatch?.Invoke(tileTypes.First(type => type.id == typeId), count);

			var matrix = Matrix;

			while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null)
			{
				Shuffle();

				matrix = Matrix;
			}
		}

		public void BoomBooster(Transform particle)
        {
			var center = _selection[0];
			_selection.Clear();

			for(int i = Mathf.Max(0, center.y - 2); i < Mathf.Min(rows.Length, center.y + 3); i++)
            {
				for (int j = Mathf.Max(0, center.x - 2); j < Mathf.Min(rows[0].tiles.Length, center.x + 3); j++)
                {
					OnMatch?.Invoke(rows[i].tiles[j].Type, 1);

					rows[i].tiles[j].Type = tileTypes[Random.Range(0, tileTypes.Length)];
				}
			}

			particle.position = center.transform.position;

			var matrix = Matrix;

			while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null)
			{
				Shuffle();

				matrix = Matrix;
			}
		}

		private TileData[,] Matrix
		{
			get
			{
				var width = rows.Max(row => row.tiles.Length);
				var height = rows.Length;

				var data = new TileData[width, height];

				for (var y = 0; y < height; y++)
					for (var x = 0; x < width; x++)
						data[x, y] = GetTile(x, y).Data;

				return data;
			}
		}

		private void Start()
		{
			for (var y = 0; y < rows.Length; y++)
			{
				for (var x = 0; x < rows.Max(row => row.tiles.Length); x++)
				{
					var tile = GetTile(x, y);

					tile.x = x;
					tile.y = y;

					tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

					tile.button.onClick.AddListener(() => Select(tile));
				}
			}

			if (ensureNoStartingMatches) StartCoroutine(EnsureNoStartingMatches());

			OnMatch += (type, count) => Debug.Log($"Matched {count}x {type.name}.");
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				var bestMove = TileDataMatrixUtility.FindBestMove(Matrix);

				if (bestMove != null)
				{
					Select(GetTile(bestMove.X1, bestMove.Y1));
					Select(GetTile(bestMove.X2, bestMove.Y2));
				}
			}

			if(selectBorder.gameObject.activeSelf && ChoiseCount == 0)
            {
				selectBorder.gameObject.SetActive(false);
            }
		}

		private IEnumerator EnsureNoStartingMatches()
		{
			var wait = new WaitForEndOfFrame();

			while (TileDataMatrixUtility.FindBestMatch(Matrix) != null)
			{
				Shuffle();

				yield return wait;
			}
		}

		private Tile GetTile(int x, int y) => rows[y].tiles[x];

		private Tile[] GetTiles(IList<TileData> tileData)
		{
			var length = tileData.Count;

			var tiles = new Tile[length];

			for (var i = 0; i < length; i++) tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

			return tiles;
		}

		private async void Select(Tile tile)
		{
			OnTileClick?.Invoke(tile);

			if (_isSwapping || _isMatching || _isShuffling) return;

			if (!_selection.Contains(tile))
			{
				if (_selection.Count > 0)
				{
					if (Math.Abs(tile.x - _selection[0].x) == 1 && Math.Abs(tile.y - _selection[0].y) == 0
					    || Math.Abs(tile.y - _selection[0].y) == 1 && Math.Abs(tile.x - _selection[0].x) == 0)
						_selection.Add(tile);
				}
				else
				{
					_selection.Add(tile);

					selectBorder.gameObject.SetActive(true);
					selectBorder.SetParent(tile.transform);

					selectBorder.anchoredPosition = Vector2.zero;
					selectBorder.localScale = Vector2.one;
				}
			}
			else
            {
				_selection.Remove(tile);

			}

			if (_selection.Count < 2) return;

			await SwapAsync(_selection[0], _selection[1]);

			if (!await TryMatchAsync()) await SwapAsync(_selection[0], _selection[1]);

			var matrix = Matrix;

			while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null)
			{
				Shuffle();

				matrix = Matrix;
			}

			_selection.Clear();
		}

		private async Task SwapAsync(Tile tile1, Tile tile2)
		{
			_isSwapping = true;

			var icon1 = tile1.icon;
			var icon2 = tile2.icon;

			var icon1Transform = icon1.transform;
			var icon2Transform = icon2.transform;

			icon1Transform.SetParent(swappingOverlay);
			icon2Transform.SetParent(swappingOverlay);

			icon1Transform.SetAsLastSibling();
			icon2Transform.SetAsLastSibling();

			var sequence = DOTween.Sequence();

			sequence.Join(icon1Transform.DOMove(icon2Transform.position, tweenDuration).SetEase(Ease.OutBack))
			        .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration).SetEase(Ease.OutBack));

			await sequence.Play()
			              .AsyncWaitForCompletion();

			icon1Transform.SetParent(tile2.transform);
			icon2Transform.SetParent(tile1.transform);

			tile1.icon = icon2;
			tile2.icon = icon1;

			var tile1Item = tile1.Type;

			tile1.Type = tile2.Type;

			tile2.Type = tile1Item;

			_isSwapping = false;
		}

		private async Task<bool> TryMatchAsync()
		{
			var didMatch = false;

			_isMatching = true;

			var match = TileDataMatrixUtility.FindBestMatch(Matrix);

			while (match != null)
			{
				didMatch = true;

				var tiles = GetTiles(match.Tiles);

				var deflateSequence = DOTween.Sequence();

				foreach (var tile in tiles) deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));

				audioSource.PlayOneShot(matchSound);

				await deflateSequence.Play()
				                     .AsyncWaitForCompletion();

				var inflateSequence = DOTween.Sequence();

				foreach (var tile in tiles)
				{
					tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

					inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));
				}

				await inflateSequence.Play()
				                     .AsyncWaitForCompletion();

				OnMatch?.Invoke(Array.Find(tileTypes, tileType => tileType.id == match.TypeId), match.Tiles.Length);

				match = TileDataMatrixUtility.FindBestMatch(Matrix);
			}

			_isMatching = false;

			return didMatch;
		}

		private void Shuffle()
		{
			_isShuffling = true;

			foreach (var row in rows)
				foreach (var tile in row.tiles)
					tile.Type = tileTypes[Random.Range(0, tileTypes.Length)];

			_isShuffling = false;
		}
	}
}
