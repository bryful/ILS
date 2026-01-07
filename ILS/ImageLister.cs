using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json.Nodes;

namespace ILS
{
	public class ImageLister
	{
		private string m_TargetDir = "";
		public string TargetDir
		{
			get { return m_TargetDir; }
			set { SetTargetDir(value); }
		}
		private List<ImageDirInfo> m_dirInfos = new List<ImageDirInfo>();
		public ImageLister() { }
		public ImageLister(string s) 
		{
			SetTargetDir( s );
			ListupFromDir(m_TargetDir);
		}
		public void SetTargetDir( string dir )
		{
			if (Directory.Exists( dir ) == true)
			{
				m_TargetDir = dir;
			}
			else
			{
				m_TargetDir = string.Empty;
			}
		}
		private bool IsImageFile( string file )
		{
			string ext = Path.GetExtension( file ).ToLower();
			if (
				ext == ".tga" ||
				ext == ".jpg" || 
				ext == ".jpeg" || 
				ext == ".png" ||
				ext == ".psd" ||
				ext == ".bmp" || 
				ext == ".tif" || 
				ext == ".tiff" || 
				ext == ".pic")
			{
				return true;
			}
			return false;
		}
		private (string name, string number) ExtractFileNameParts(string fileName)
		{
			string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
			Match match = Regex.Match(nameWithoutExt, @"^(.+?)(\d+)$");

			if (match.Success)
			{
				return (match.Groups[1].Value, match.Groups[2].Value);
			}

			return (nameWithoutExt, string.Empty);
		}
		private bool ListupFromDirSub( string dir )
		{
			if ( Directory.Exists( dir ) == false )
			{
				return false;
			}
			var files = Directory.EnumerateFiles( dir );

			ImageDirInfo dirInfo = new ImageDirInfo();
			List<ImageDirInfo> tempDirInfos = new List<ImageDirInfo>();
			int count = 0;
			foreach ( string file in files )
			{
				if (IsImageFile(file) == false) continue;
				var (name, number) = ExtractFileNameParts(file);
				if (dirInfo.NodeName == "")
				{
					dirInfo.FullDirName = dir;
					dirInfo.DirName = Path.GetFileName( dir );
					dirInfo.NodeName = name;
					dirInfo.AddItem( Path.GetFileName(file) );
				}
				else
				{
					if (dirInfo.NodeName!=name)
					{
						tempDirInfos.Add( dirInfo );
						dirInfo = new ImageDirInfo();
						dirInfo.FullDirName = dir;
						dirInfo.DirName = Path.GetFileName( dir );
						dirInfo.NodeName = name;
						dirInfo.AddItem( Path.GetFileName(file) );
					}
					else
					{
						dirInfo.AddItem(Path.GetFileName(file));
					}
				}
				count++;
			}
			if (dirInfo.Items.Count > 0)
			{
				tempDirInfos.Add(dirInfo);
				count++;
			}
			m_dirInfos.AddRange( tempDirInfos );
			return (count>0);
		}
		public bool ListupFromDir(string dir)
		{
			bool ret = false;
			if (dir == "") return ret;
			m_dirInfos.Clear();
			int count = 0;
			if ( ListupFromDirSub( dir ) == true )
			{
				count++;
			}
			var dirs = Directory.EnumerateDirectories( dir );
			foreach ( string subDir in dirs )
			{
				if ( ListupFromDirSub( subDir ) == true )
				{
					count++;
				}
			}
			ret = (count > 0);
			return ret;
		}
		public JsonArray ToJsonArray()
		{
			var ret = new JsonArray();
			foreach ( var dirInfo in m_dirInfos )
			{
				ret.Add( dirInfo.ToJsonObject() );
			}
			return ret;
		}
		public void Clear()
		{
			m_dirInfos.Clear();
		}
		public string ToJsonString()
		{
			var options = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			var jsonArray = ToJsonArray();
			return JsonSerializer.Serialize( jsonArray, options );
		}
		public string Exec(string path)
		{
			if ( ListupFromDir( path ) == false )
			{
				return "";
			}
			return ToJsonString();
		}
		public string Exec()
		{
			return Exec( m_TargetDir );
		}
	}

	public class ImageDirInfo
	{
		public string FullDirName { get; set; } = "";
		public string DirName { get; set; } = "";
		public string NodeName { get; set; } = "";
		public List<string> Items { get; set; } = new List<string>();
		public ImageDirInfo() { }
		public void ClearItems()
		{
			Items.Clear();
		}
		public int IndexOf( string item )
		{
			return Items.IndexOf( item );
		}
		public bool AddItem( string item )
		{
			if (item == "")
			{
				return false;
			} 
			int idx = Items.IndexOf( item );
			if (idx >= 0)
			{
				return false;
			}
			Items.Add( item );
			return true;
		}
		public JsonObject ToJsonObject()
		{
			var ret = new JsonObject();
			ret["fullDirName"] = FullDirName;
			ret["dirName"] = DirName;
			ret["nodeName"] = NodeName;
			var itemsArray = new JsonArray();
			foreach ( var item in Items )
			{
				itemsArray.Add( item );
			}
			ret["items"] = itemsArray;
			return ret;
		}
	}
}
