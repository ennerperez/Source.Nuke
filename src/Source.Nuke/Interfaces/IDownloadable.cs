namespace Nuke.Source.Interfaces
{
	public interface IDownloadable
	{
		string Url { get; }
		bool Download();
	}
}
