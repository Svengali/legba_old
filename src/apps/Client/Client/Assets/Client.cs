using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class Client : MonoBehaviour
{
	// Start is called before the first frame update
	void Start()
	{


	}

	bool m_hasStarted = false;

	List<Component> m_components = new List<Component>();

	// Update is called once per frame
	void Update()
	{
		if(!m_hasStarted)
		{
			m_hasStarted = true;

			m_components.AddRange( gameObject.GetComponents<Component>() );

			const string FMT = "yyyyMMddHHmmss";

			var time = DateTime.Now.ToString(FMT);

			var formatter = new lib.XmlFormatter2("unknown");

			using(var stream = new FileStream($"XMLTest_{time}.xml", FileMode.Create, FileAccess.Write))
			{
				formatter.Serialize(stream, this);
			}
		}
	}



	net.Conn m_connection;

}
