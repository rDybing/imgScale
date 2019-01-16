using System;
using static System.Console;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace imgScale {

	struct image {
		public int width, height;
		public image(int width, int height) {
			this.width = width;
			this.height = height;
   		}
	};
    class Program {
		static void Main(){

			string[] outFile = new string[4];
			string[] inFile = {
				"test_iOS_24b.jpg",
				"test_iOS_24b_half.png",
				"test_iOS_32b.png",
				"test_iOS_32b_half.png"
			};
			string inDir = @"imgPre\";
			string outDir = @"imgPostScale\";
			string cropDir = @"imgPostCrop\";

			image newImg = new image(0,0);
			image oldImg = new image(0, 0); 

			for(int i = 0; i < inFile.Length; i++) {
				outFile[i] = newSuffix(inFile[i]);
				WriteLine(inDir + inFile[i]);
				oldImg = getDimensions(inDir + inFile[i]);
				WriteLine("old - w: {0:D4} :: h: {1:D4}", oldImg.width, oldImg.height);
				newImg = calcNewSize(oldImg);
				WriteLine("new - w: {0:D4} :: h: {1:D4}", newImg.width, newImg.height);
				scaleNewFile(inDir+inFile[i], outDir+outFile[i], newImg);
				cropNewFile(inDir+inFile[i], cropDir+outFile[i], newImg);
			}
		}
		static string newSuffix(string input) {
			StringBuilder output = new StringBuilder();
			if (input.EndsWith(".png")) {
				output.Append(input.Substring(0, input.Length - ".png".Length));
			}
			if (input.EndsWith(".jpg")) {
				output.Append(input.Substring(0, input.Length - ".jpg".Length));
			}
			output.Append(".small.png");
			return output.ToString();
		}

		static image getDimensions(string inFile){
			image imgOut = new image(0,0);
            var process = new Process();
			string exe = "cmd.exe";
			string cmd = $"/C ffprobe -v error -show_entries stream=width,height -of default=noprint_wrappers=1 {inFile}";
            process.StartInfo.FileName = exe;
            process.StartInfo.Arguments = cmd;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            StreamReader reader = process.StandardOutput;
            string output = string.Empty;
			string width = string.Empty;
			string height = string.Empty;
			int index = 0;
            while ((output = reader.ReadLine()) != null) {
                if (output.Contains("width=")) {
					index = output.LastIndexOf('=');
					width = output.Substring(index + 1);
                    int.TryParse(width, out imgOut.width);
				}
                if (output.Contains("height=")) {
					index = output.LastIndexOf('=');
					height = output.Substring(index + 1);
                    int.TryParse(height, out imgOut.height);
				}
            }
            process.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
            return imgOut;
        }

		static image calcNewSize(image input) {
			image output = new image(0,0);
			output.height = 256;
			float scaleValue = (float)output.height / (float)input.height;
			float width = (float)input.width * scaleValue;
			output.width = (int)width;
			if (output.width % 2 != 0) {
				output.width++;
			}
			while (output.width%16 != 0) {
				output.width += 2;
			}
			return output;
		}

		static void scaleNewFile(string inFile, string outFile, image img) {
			var process = new Process();
			string height = img.height.ToString();
			string width = img.width.ToString();
			string exe = "cmd.exe";
			string cmd = $"/C ffmpeg -i {inFile} -vf scale=(iw*sar)*max({width}/(iw*sar)\\,{height}/ih):ih*max({width}/(iw*sar)\\,{height}/ih) {outFile}";
			process.StartInfo.FileName = exe;
            process.StartInfo.Arguments = cmd;
            process.StartInfo.UseShellExecute = false;
        	process.StartInfo.RedirectStandardOutput = true;
        	process.StartInfo.RedirectStandardError = true;
        	process.StartInfo.CreateNoWindow = true;
            process.Start();
			process.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
			WriteLine("Scale Success!");
			// save to blob after this			
		}

		static void cropNewFile(string inFile, string outFile, image img) {
			var process = new Process();
			string height = img.height.ToString();
			string exe = "cmd.exe";
			string cmd = $"/C ffmpeg -i {inFile} -vf \"scale=(iw*sar)*max({height}/(iw*sar)\\,{height}/ih):ih*max({height}/(iw*sar)\\,{height}/ih), crop=256:256\" {outFile}";
			process.StartInfo.FileName = exe;
            process.StartInfo.Arguments = cmd;
            process.StartInfo.UseShellExecute = false;
        	process.StartInfo.RedirectStandardOutput = true;
        	process.StartInfo.RedirectStandardError = true;
        	process.StartInfo.CreateNoWindow = true;
            process.Start();
			process.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
			WriteLine("Scale and Crop Success!");
			// save to blob after this			
		}
	}
}
