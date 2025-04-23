using System.Collections;

public class ApiGetRequest : ApiRequest{

    public ApiGetRequest(string gameId, string playerId)
    {
        this.gameId = gameId;
        this.playerId = playerId;
    }

    public override IEnumerator Execute(ApiClient client)
    {
        yield return client.PerformGet(gameId, playerId);
    }
}