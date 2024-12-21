using LMKit.Model;
using System.Web;

namespace LMKit.Maestro.Helpers;

public static class FileHelpers
{
    public static bool GetModelInfoFromDownloadUrl(Uri url, out string publisher, out string repository, out string fileName)
    {
        if (!url.IsFile && url.Segments.Length >= 3)
        {
            publisher = SanitizeUriSegment(url.Segments[1]);
            repository = SanitizeUriSegment(url.Segments[2]);
            fileName = SanitizeUriSegment(url.Segments[url.Segments.Length - 1]);

            return true;
        }

        repository = string.Empty;
        publisher = string.Empty;
        fileName = string.Empty;

        return false;
    }

    public static bool GetModelInfoFromPath(string filePath, string modelsRootFolder, out string publisher, out string repository, out string fileName)
    {
        Uri fileUri;

        try
        {
            fileUri = new Uri(filePath);
        }
        catch (Exception)
        {
            repository = string.Empty;
            publisher = string.Empty;
            fileName = string.Empty;

            return false;
        }

        return GetModelInfoFromFileUri(fileUri, modelsRootFolder, out publisher, out repository, out fileName);
    }

    public static bool GetModelInfoFromFileUri(Uri fileUri, string modelsRootFolder, out string publisher, out string repository, out string fileName)
    {
        Uri modelsFolderUri = new Uri(modelsRootFolder);

        if (fileUri.IsFile && modelsFolderUri.IsBaseOf(fileUri) && fileUri.Segments.Length - modelsFolderUri.Segments.Length == 3)
        {
            publisher = SanitizeUriSegment(fileUri.Segments[fileUri.Segments.Length - 3]);
            repository = SanitizeUriSegment(fileUri.Segments[fileUri.Segments.Length - 2]);
            fileName = SanitizeUriSegment(fileUri.Segments[fileUri.Segments.Length - 1]);

            return true;
        }

        repository = string.Empty;
        publisher = string.Empty;
        fileName = string.Empty;

        return false;
    }

    public static bool TryCreateFileUri(string filePath, out Uri? uri)
    {
        try
        {
            uri = new Uri(filePath);
            return true;
        }
        catch
        {
            uri = null;
            return false;
        }
    }

    public static bool GetModelPublisherAndRepositoryFromFolderPath(string folderPath, string modelsRootFolder, out string publisher, out string repository)
    {
        Uri modelsRootFolderUri = new Uri(modelsRootFolder);
        Uri folderPathUri = new Uri(folderPath);

        if (modelsRootFolderUri.IsBaseOf(folderPathUri) && folderPathUri.Segments.Length - modelsRootFolderUri.Segments.Length == 2)
        {
            publisher = SanitizeUriSegment(modelsRootFolderUri.Segments[folderPathUri.Segments.Length - 2]);
            repository = SanitizeUriSegment(modelsRootFolderUri.Segments[folderPathUri.Segments.Length - 1]);

            return true;
        }

        repository = string.Empty;
        publisher = string.Empty;

        return false;
    }

    public static string SanitizeUriSegment(string uriSegment)
    {
        if (uriSegment.EndsWith('/'))
        {
            uriSegment = uriSegment.Substring(0, uriSegment.Length - 1);
        }

        uriSegment = HttpUtility.UrlDecode(uriSegment);

        return Uri.UnescapeDataString(uriSegment);
    }

    public static Uri GetModelFileUri(ModelCard modelCard, string modelsFolderPath)
    {
        return new Uri(GetModelFilePath(modelCard, modelsFolderPath));
    }

    public static string GetModelFilePath(ModelCard modelCard, string modelsFolderPath)
    {
        return Path.Combine(modelsFolderPath, modelCard.Publisher, modelCard.Repository, modelCard.FileName);
    }

    public static string? GetModelFilePathFromUrl(Uri modelUrl, string modelsFolderPath)
    {
        if (GetModelInfoFromDownloadUrl(modelUrl, out string publisher, out string repository, out string fileName))
        {
            return Path.Combine(modelsFolderPath, publisher, repository, fileName);
        }

        return null;
    }

    public static string GetFileBaseName(Uri fileUri)
    {
        return SanitizeUriSegment(fileUri.Segments[fileUri.Segments.Length - 1]);
    }

    public static Uri GetRenamedFileUri(Uri oldFileUri, string newFileBaseName)
    {
        return GetRenamedFileUri(oldFileUri, newFileBaseName, 0);
    }

    public static Uri GetRenamedFileUri(Uri oldFileUri, string renamedElement, int ancestorLevel)
    {
        var builder = new UriBuilder(oldFileUri);
        var uriSegments = builder.Path.Split('/');

        uriSegments[uriSegments.Length - ancestorLevel - 1] = renamedElement;
        builder.Path = string.Join('/', uriSegments);

        return builder.Uri;
    }

    public static async Task<long?> GetFileSizeFromUri(Uri uri)
    {
        try
        {
            if (uri.IsFile)
            {
                return GetFileSize(uri.LocalPath);
            }
            else
            {
                using HttpClient httpClient = new HttpClient();
                using HttpResponseMessage httpResponse = await httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

                return httpResponse.Content.Headers.ContentLength;
            }
        }
        catch
        {
            return null;
        }
    }

    public static long GetFileSize(string path)
    {
        return new FileInfo(path).Length;
    }

    public static string GetModelFileRelativeName(ModelCard modelCard)
    {
        return Path.Combine(modelCard.Publisher, modelCard.Repository, modelCard.FileName);
    }

    public static string GetModelFileRelativePath(string filePath, string modelsFolderPath)
    {
        if (filePath.Contains(modelsFolderPath))
        {
            return filePath.Substring(modelsFolderPath.Length + 1);
        }

        return filePath;
    }

    public static bool IsFileDirectory(string filePath)
    {
        try
        {
            return File.GetAttributes(filePath).HasFlag(FileAttributes.Directory);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsFileLocked(string filePath)
    {
        try
        {
            using (FileStream stream = new FileInfo(filePath).Open(FileMode.Open, FileAccess.Read, FileShare.None))
            {
                stream.Close();
            }
        }
        catch (IOException)
        {
            return true;
        }

        return false;
    }
}
