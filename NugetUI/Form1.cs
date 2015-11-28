using System;
using System.Linq;
using System.Windows.Forms;

namespace NugetUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        class Spec
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public string Company { get; set; }
            public string FileVersion { get; set; }
            public string Version { get; set; }
            public string ReleaseNotes { get; set; }
            public string Tags { get; set; }
            public string ProjectUrl { get; set; }
            public string LicenseUrl { get; set; }
            public string IconUrl { get; set; }
            public string Copyright { get; set; }

        }

        string directory = "";

        string specFormat = @"<?xml version=""1.0""?>
<package>
<metadata>
<id>$id$</id>
<version>$version$</version>
<title> $title$ </title>
<authors> $author$ </authors>
<owners> $author$ </owners>
<licenseUrl> http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE</licenseUrl>
<projectUrl> http://Project_URL_HERE_OR_DELETE_THIS_LINE</projectUrl>
<iconUrl> http://ICON_URL_HERE_OR_DELETE_THIS_LINE</iconUrl>
<requireLicenseAcceptance> false </requireLicenseAcceptance>
<description>$des$</description>
<releaseNotes>$note$</releaseNotes>
<copyright> Copyright 2015 </copyright>
<tags>$tags$</tags>
</metadata>
</package>";

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                txtCsproj.Text = openFileDialog1.FileName;
                directory = System.IO.Path.GetDirectoryName(openFileDialog1.FileName);

                Spec spec = FindInfoFile();
                if (spec == null)
                    return;

                ReadSpecFile(spec);

                UpdateControls(spec);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            if(!UpdateSpec())
            {
                MessageBox.Show("error");
                return;
            }

            string path = Environment.CurrentDirectory.ToString() + "\\nuget.exe";

            if (!System.IO.File.Exists(directory + "\\nuget.exe"))
                System.IO.File.Copy(path, directory + "\\nuget.exe");


            System.Diagnostics.Process nuget = new System.Diagnostics.Process();
            nuget.StartInfo.FileName = directory + "\\nuget.exe";
            nuget.StartInfo.RedirectStandardOutput = true;
            nuget.StartInfo.UseShellExecute = false;
            nuget.StartInfo.WorkingDirectory = directory;
            nuget.StartInfo.Arguments = "pack ";//+ System.IO.Path.GetFileName(txtCsproj.Text);
            nuget.Start();

            var aa = nuget.StandardOutput.ReadToEnd();
            listBox1.Items.Add(nuget.StandardOutput.ReadToEnd());



        }


        private Spec FindInfoFile()
        {
            string[] files = System.IO.Directory.GetFiles(directory, "*.cs", System.IO.SearchOption.AllDirectories);
            foreach (string temp in files)
            {
                if (temp.Split((char)(92)).Last() == "AssemblyInfo.cs")
                {
                    return ReadInfoFile(temp);
                }
            }

            return null;
        }

        private Spec ReadInfoFile(string path)
        {

            System.IO.StreamReader sr = new System.IO.StreamReader(path);
            string data = "";

            Spec specInfo = new Spec();

            System.Text.RegularExpressions.Regex regx = new System.Text.RegularExpressions.Regex(@""".*""");

            while (!sr.EndOfStream)
            {
                data = sr.ReadLine();

                if (data.Contains("AssemblyTitle"))
                {
                    specInfo.Title = regx.Match(data).Value.Replace('"', ' ').Trim();
                }

                if (data.Contains("AssemblyDescription"))
                {
                    specInfo.Description = regx.Match(data).Value.Replace('"', ' ').Trim(); ;
                }

                if (data.Contains("AssemblyCompany"))
                {
                    specInfo.Company = regx.Match(data).Value.Replace('"', ' ').Trim(); ;
                }

                if (data.Contains("AssemblyVersion"))
                {
                    specInfo.Version = regx.Match(data).Value.Replace('"', ' ').Trim(); ;
                }

                if (data.Contains("AssemblyFileVersion"))
                {
                    specInfo.FileVersion = regx.Match(data).Value.Replace('"', ' ').Trim(); ;
                }

                if (data.Contains("AssemblyCopyright"))
                {
                    specInfo.Copyright = regx.Match(data).Value.Replace('"', ' ').Trim(); ;
                }
            }

            return specInfo;
        }

        private void ReadSpecFile(Spec spec)
        {

            var path = directory + "\\" + System.IO.Path.GetFileName(txtCsproj.Text).Replace(".csproj", "") + ".nuspec";
            if (!System.IO.File.Exists(path))
            {
                specFormat = specFormat.Replace("$title$", spec.Title);
                specFormat = specFormat.Replace("$author$", spec.Company);
                specFormat = specFormat.Replace("$des$", spec.Description);
                specFormat = specFormat.Replace("$note$", spec.ReleaseNotes);
                specFormat = specFormat.Replace("$version$", spec.Version);
                specFormat = specFormat.Replace("$tags$", spec.Tags);
                specFormat = specFormat.Replace("http://Project_URL_HERE_OR_DELETE_THIS_LINE", spec.ProjectUrl);
                specFormat = specFormat.Replace("http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE", spec.LicenseUrl);
            }
            else
            {
                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@".*>.*<\s*/.*>", System.Text.RegularExpressions.RegexOptions.Singleline);
                System.Text.RegularExpressions.Regex regex2 = new System.Text.RegularExpressions.Regex(@"(<\s*/?\s*\w+\s*>)", System.Text.RegularExpressions.RegexOptions.Singleline);

                System.IO.StreamReader sr = new System.IO.StreamReader(path);
                while (!sr.EndOfStream)
                {
                    string data = sr.ReadLine().Trim().Replace(" ", "");

                    if (data.Contains("<version>"))
                    {
                        var temp = regex.Match(data).Value;
                        if (string.IsNullOrEmpty(temp))
                            continue;

                        temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        if (temp != "$version$")
                        {
                            spec.Version = temp;
                        }
                    }

                    if (data.Contains("<title>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        if (temp != "$title$")
                        {
                            spec.Title = temp;
                        }
                    }

                    if (data.Contains("<authors>") || data.Contains("<owners>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        if (temp != "$author$")
                            spec.Company = temp;
                    }

                    if (data.Contains("<licenseUrl>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        // if (temp != "http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE")
                        spec.LicenseUrl = temp;
                    }

                    if (data.Contains("<projectUrl>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        // if (temp != "http://PROJECT_URL_HERE_OR_DELETE_THIS_LINE")
                        spec.ProjectUrl = temp;
                    }

                    if (data.Contains("<iconUrl>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        // if (temp != "http://ICON_URL_HERE_OR_DELETE_THIS_LINE")
                        spec.IconUrl = temp;
                    }


                    //if (data.Contains("<requireLicenseAcceptance>"))
                    //{
                    //    var temp = regex.Match(data).Value;
                    //    temp = regex2.Replace(temp, "", 2, 0);
                    //    //if (temp != "$version$")
                    //    //    spec.Version = temp;
                    //}

                    if (data.Contains("<description>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        if (temp != "$description$")
                            spec.Description = temp;
                    }

                    if (data.Contains("<releaseNotes>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0);
                        if (temp != "Summaryofchangesmadeinthisreleaseofthepackage.")
                            spec.ReleaseNotes = temp;
                    }

                    //if (data.Contains("<copyright>"))
                    //{
                    //    var temp = regex.Match(data).Value;
                    //    temp = regex2.Replace(temp, "", 2, 0);
                    //    spec.ReleaseNotes = temp;
                    //}

                    if (data.Contains("<tags>"))
                    {
                        var temp = regex.Match(data).Value;
                        temp = regex2.Replace(temp, "", 2, 0); ;
                        if (temp != "Tag1Tag2")
                            spec.Tags = temp;
                    }
                }

                sr.Close();
            }
        }

        private void UpdateControls(Spec spec)
        {
            txtTitle.Text = spec.Title;
            txtVersion.Text = spec.Version;
            txtFileVersion.Text = spec.FileVersion;
            txtCompany.Text = spec.Company;
            richDescription.Text = spec.Description;
            txtCopyright.Text = spec.Copyright;
            txtLicenseUrl.Text = spec.LicenseUrl;
            txtProjectUrl.Text = spec.ProjectUrl;
            txtIconUrl.Text = spec.IconUrl;
            txtTags.Text = spec.Tags;
            richNote.Text = spec.ReleaseNotes;
        }

        private bool UpdateSpec()
        {

            specFormat = specFormat.Replace("$version$", txtVersion.Text);
            specFormat = specFormat.Replace("$title$", txtTitle.Text);
            specFormat = specFormat.Replace("$author$", txtCompany.Text);

            if (string.IsNullOrEmpty(txtLicenseUrl.Text))
                specFormat = specFormat.Replace("http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE", txtLicenseUrl.Text);
            if (string.IsNullOrEmpty(txtProjectUrl.Text))
                specFormat = specFormat.Replace("http://Project_URL_HERE_OR_DELETE_THIS_LINE", txtProjectUrl.Text);
            if (string.IsNullOrEmpty(txtIconUrl.Text))
                specFormat = specFormat.Replace(" http://ICON_URL_HERE_OR_DELETE_THIS_LINE", txtIconUrl.Text);


            specFormat = specFormat.Replace("$des$", richDescription.Text);
            specFormat = specFormat.Replace("$note$", richNote.Text);
            specFormat = specFormat.Replace("Copyright 2015", txtCopyright.Text);
            specFormat = specFormat.Replace("$tags$", txtTags.Text);

            var path = directory + "\\" + System.IO.Path.GetFileName(txtCsproj.Text).Replace(".csproj", "") + ".nuspec";
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);


            System.IO.StreamWriter sw = new System.IO.StreamWriter(path);
            sw.Write(specFormat);
            sw.Close();
            return true;
        }
    }
}
