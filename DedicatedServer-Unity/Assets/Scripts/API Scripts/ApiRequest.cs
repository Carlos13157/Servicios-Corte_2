using System.Collections;

public abstract class ApiRequest{
    protected string gameId;
    protected string playerId;
    public abstract IEnumerator Execute(ApiClient client);
}