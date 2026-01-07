using System;
using System.IO;
using ILS;
/*
 * 	//jsonを使うおまじない
	var json_file = new File(Folder.appPackage.fsName + '/Libraries/jsx/json2.jsx');
	if(json_file.exists && (typeof JSON !== 'object')) $.evalFile(json_file);

	var result = Syste.callSystem("ils D:\Work\paint\EE01_001_p1");
	var obj = JSON.parse(result);

 */
string targetDir = @"";

	targetDir = Directory.GetCurrentDirectory();
	ImageLister lister = new ImageLister();
	if (args.Length>0)
	{
		targetDir = args[0];
	}
	lister.TargetDir = targetDir;

	if (lister.TargetDir == "")
	{
		String str = "ils.exe 指定したフォルダのファイル攻勢をJSON形式で変えす";
		Console.WriteLine(str);
	}
	else {

		Console.WriteLine(lister.Exec());
	}


