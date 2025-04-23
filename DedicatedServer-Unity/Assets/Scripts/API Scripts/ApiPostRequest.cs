using System.Collections;

public class ApiPostRequest<T> : ApiRequest
{
    private T data;

    public ApiPostRequest(string gameId, string playerId, T data)
    {
        this.gameId = gameId;
        this.playerId = playerId;
        this.data = data;
    }

    public override IEnumerator Execute(ApiClient client)
    {
        yield return client.PerformPost<T>(gameId, playerId, data); // Usar método genérico
    }
}
