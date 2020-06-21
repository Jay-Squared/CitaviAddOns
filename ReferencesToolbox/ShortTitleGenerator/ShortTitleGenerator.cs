using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SwissAcademic.Citavi;
using SwissAcademic.Citavi.Citations;

using SwissAcademic.Citavi.Shell;

namespace ShortTitleGenerator
{
    class ShortTitleGenerator
        :
        IReferenceStringPropertyFilter
    {
        public String GetFilterResult(Reference reference, out bool handled)
        {
            handled = true;
			string newShortTitle = null;

			if (Form.ActiveForm == Program.ActiveProjectShell.PrimaryMainForm)
			{
				newShortTitle = NewShortTitle(reference);

			}
			else
			{
				return null;
			}

			return newShortTitle;

		}
		public static string NewShortTitle(Reference reference)
		{
			try
			{
				if (reference == null) return null;

				string newShortTitle = string.Empty;

				Project project = Program.ActiveProjectShell.Project;

				string styleName = "ShortTitleGenerator.ccs";
				string folder = project.Addresses.GetFolderPath(CitaviFolder.UserData).ToString();
				string fullPath = folder + @"\Custom Citation Styles\" + styleName;
				if (!System.IO.File.Exists(fullPath))
				{
					System.Windows.Forms.MessageBox.Show(ShortTitleGeneratorLocalization.CitationStyleNotFound);
					return null;
				}
				Uri uri = new Uri(fullPath);

				CitationStyle citationStyle = CitationStyle.Load(uri.AbsoluteUri);

				List<Reference> references = new List<Reference>();
				references.Add(reference);

				CitationManager citationManager = new CitationManager(Program.ActiveProjectShell.Project.Engine, citationStyle, references);
				if (citationManager == null) return null;

				BibliographyCitation bibliographyCitation = citationManager.BibliographyCitations.FirstOrDefault();
				if (bibliographyCitation == null) return null;

				List<ITextUnit> textUnits = bibliographyCitation.GetTextUnits();
				if (textUnits == null) return null;

				var output = new TextUnitCollection();

				foreach (ITextUnit textUnit in textUnits)
				{
					output.Add(textUnit);
				}

				newShortTitle = output.ToString();
				return newShortTitle;
			}
			catch { return null; }
		}
	}
}
