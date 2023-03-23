
using System;
using System.Collections.Generic;
using System.Reactive;
using Avalonia.Media.Imaging;
using DungeonGenerator;
using DungeonViewer.Utils;
using ReactiveUI;
using SkiaSharp;

namespace DungeonViewer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _roomMinWidth;
        public int RoomMinWidth
        {
            get => _roomMinWidth;
            set
            {
                this.RaiseAndSetIfChanged(ref _roomMinWidth, value);
                _map.RoomMinWidth = value;
                GenerateMap();
            }
        }

        private int _roomMaxWidth;
        public int RoomMaxWidth
        {
            get => _roomMaxWidth;
            set
            {
                this.RaiseAndSetIfChanged(ref _roomMaxWidth, value);
                _map.RoomMaxWidth = value;
                GenerateMap();
            }
        }

        private int _roomMinHeight;
        public int RoomMinHeight
        {
            get => _roomMinHeight;
            set
            {
                this.RaiseAndSetIfChanged(ref _roomMinHeight, value);
                _map.RoomMinHeight = value;
                GenerateMap();
            }
        }

        private int _roomMaxHeight;
        public int RoomMaxHeight
        {
            get => _roomMaxHeight;
            set
            {
                this.RaiseAndSetIfChanged(ref _roomMaxHeight, value);
                _map.RoomMaxHeight = value;
                GenerateMap();
            }
        }

        private int _roomMinCount;
        public int RoomMinCount
        {
            get => _roomMinCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _roomMinCount, value);
                _map.RoomMinCount = value;
                GenerateMap();
            }
        }

        private int _roomMaxCount;
        public int RoomMaxCount
        {
            get => _roomMaxCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _roomMaxCount, value);
                _map.RoomMaxCount = value;
                GenerateMap();
            }
        }

        private int _minDistanceBetweenRooms;
        public int MinDistanceBetweenRooms
        {
            get => _minDistanceBetweenRooms;
            set
            {
                this.RaiseAndSetIfChanged(ref _minDistanceBetweenRooms, value);
                _map.MinDistanceBetweenRooms = value;
                GenerateMap();
            }
        }

        private int _maxDistanceBetweenRooms;
        public int MaxDistanceBetweenRooms
        {
            get => _maxDistanceBetweenRooms;
            set
            {
                this.RaiseAndSetIfChanged(ref _maxDistanceBetweenRooms, value);
                _map.MaxDistanceBetweenRooms = value;
                GenerateMap();
            }
        }

        private int _minPassWidth;
        public int MinPassWidth
        {
            get => _minPassWidth;
            set
            {
                this.RaiseAndSetIfChanged(ref _minPassWidth, value);
                _map.MinPassWidth = value;
                GenerateMap();
            }
        }

        private int _maxPassWidth;
        public int MaxPassWidth
        {
            get => _maxPassWidth;
            set
            {
                this.RaiseAndSetIfChanged(ref _maxPassWidth, value);
                _map.MaxPassWidth = value;
                GenerateMap();
            }
        }

        private int _seed;
        public int Seed
        {
            get => _seed;
            set
            {
                this.RaiseAndSetIfChanged(ref _seed, value);
                GenerateMap();
            }
        }

        private Bitmap _image;
        public Bitmap Image
        {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }

        private byte[,]? _mapArray;
        public byte[,]? MapArray
        {
            get => _mapArray;
            set => this.RaiseAndSetIfChanged(ref _mapArray, value);
        }

        private bool _fixSeed;
        public bool FixSeed
        {
            get => _fixSeed;
            set => this.RaiseAndSetIfChanged(ref _fixSeed, value);
        }

        public ReactiveCommand<Unit, Unit> GenerateCmd { get; }



        private Map _map;

        private Dictionary<int, SKColor> ColorDict;

        public MainWindowViewModel()
        {
            _roomMinWidth            = 10;
            _roomMinHeight           = 10;
            _roomMaxWidth            = 30;
            _roomMaxHeight           = 30;
            _roomMinCount            = 5;
            _roomMaxCount            = 10;
            _minDistanceBetweenRooms = 15;
            _maxDistanceBetweenRooms = 30;
            _minPassWidth            = 1;
            _maxPassWidth            = 3;
            _seed                    = 1;
            FixSeed                  = true;
            GenerateCmd              = ReactiveCommand.Create(OnGenerate);
            _map                     = new Map(_roomMinWidth, _roomMinHeight, _roomMaxWidth, _roomMaxHeight, _roomMinCount, _roomMaxCount, _minDistanceBetweenRooms, _maxDistanceBetweenRooms, _minPassWidth, _maxPassWidth);
            ColorDict                = new Dictionary<int, SKColor> {{0, new SKColor(0x22, 0x22, 0x22)}, {1, new SKColor(0x77, 0x77, 0x77)}, { 2, new SKColor(0xaa, 0x66, 0x66) }, { 3, new SKColor(0x66, 0xaa, 0x66) }, { 4, new SKColor(0x66, 0x66, 0xaa) } };
            GenerateMap();
        }

        private void OnGenerate()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            if (!FixSeed)
            {
                _seed = new Random().Next(int.MinValue, int.MaxValue);
                this.RaisePropertyChanged(nameof(Seed));
            }

            MapArray = _map.GenerateMap(_seed);
            if (MapArray == null)
            {
                var sbmp = new Sbmp(10, 10);
                sbmp.Fill(SKColors.Red);
                Image = sbmp.GetBitmap;
            }
            else
            {
                var sbmp = new Sbmp(MapArray.GetLength(0), MapArray.GetLength(1));
                for (var i = 0; i < MapArray.GetLength(0); i++)
                {
                    for (var j = 0; j < MapArray.GetLength(1); j++)
                    {
                        sbmp.DrawPixel(new SKPointI(i, j), ColorDict[MapArray[i, j]]);
                    }
                }

                Image = sbmp.GetBitmap;
                sbmp.Save("I:\\test.png");
            }
        }
    }
}