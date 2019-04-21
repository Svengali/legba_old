using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Client : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var formatter = new lib.XmlFormatter2();

				var stream = new MemoryStream();

				formatter.Serialize( stream, this );
    }

    // Update is called once per frame
    void Update()
    {
        
    }



		net.Conn m_connection;
		
}
