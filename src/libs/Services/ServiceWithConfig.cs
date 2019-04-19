














namespace svc
{




public class ServiceWithConfig<TCfg> : Service where TCfg : class
{
	public res.Ref<TCfg> cfg { get; protected set; }

	public ServiceWithConfig( lib.Token _id, res.Ref<TCfg> _cfg ) : base(_id)
	{
		cfg = _cfg;
	}
}







}
