using System.Net;

namespace BasicApp.Helpers.SharedModels;

[Serializable]
public partial class Response<T> where T : class
{
    private bool _isSuccessful = true;
    public HttpStatusCode? StatusCode { get; set; }
    public bool isSuccessful { get { return _isSuccessful; } set { _isSuccessful = value; } }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

[Serializable]
public partial class Response
{
    private bool _isSuccessful = true;
    public HttpStatusCode? StatusCode { get; set; }
    public bool isSuccessful { get { return _isSuccessful; } set { _isSuccessful = value; } }
    public string? Message { get; set; }
}
