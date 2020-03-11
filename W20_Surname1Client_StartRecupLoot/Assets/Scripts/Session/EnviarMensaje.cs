using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class EnviarMensaje : MonoBehaviour
{
    public InputField mensajeInputField;
    public TextMeshProUGUI listadoChat;

    public Player player;


    // Start is called before the first frame update
    void Start()
    {
        //Obtener la información del Player ya creado
        player = FindObjectOfType<Player>();
        string men = "Mensajes : ";
        listadoChat.text = men;
        RecibirMensajes(listadoChat);
    }

    public void OnSendMissageClick()
    {
        //Cuando se realiza click para enviar un mensaje
        StartCoroutine(Mensaje(mensajeInputField.text , listadoChat));
    }



    internal static IEnumerator Mensaje(string mensaje , TextMeshProUGUI listadoChat)
    {
        if (mensaje != "" && mensaje != null) {
            Player player = FindObjectOfType<Player>();

            //Otra forma del ID
            // Helper.GetPlayerId;

            MensajeJson mensajeSerializable = new MensajeJson();
            mensajeSerializable.IdPlayer = player.Id;
            mensajeSerializable.Mensaje = mensaje;

            using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Chat/NuevoMensaje", "POST"))
            {
                string mensajeData = JsonUtility.ToJson(mensajeSerializable);

                byte[] bodyRaw = Encoding.UTF8.GetBytes(mensajeData);
                httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);

                httpClient.SetRequestHeader("Content-type", "application/json");
                httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

                //UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Chat/NuevoMensaje", "POST");
                /*
                // application/x-www-form-urlencoded
                WWWForm dataToSend = new WWWForm();
                    //Para lo de los 3 primeros digitos del id de Player podemos directamente introducir solo los 3 primeros digitos.
                 string identificador = player.Id;
                identificador.Substring(1,3);
                    //Comprobar si funciona cuando se inserte mensajes
                   // dataToSend.AddField("IdPlayer", identificador);
                dataToSend.AddField("IdPlayer", player.Id);
                //Recogemos el mensaje que ha recibido.
                dataToSend.AddField("Mensaje", mensaje);

                httpClient.uploadHandler = new UploadHandlerRaw(dataToSend.data);
                httpClient.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                httpClient.downloadHandler = new DownloadHandlerBuffer();

                 httpClient.SetRequestHeader("Accept", "application/json");

                    //Pone unauthorized ...
                 httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
                */

                yield return httpClient.SendWebRequest();

                if (httpClient.isNetworkError || httpClient.isHttpError)
                {
                    throw new Exception("Problema al enviar el Mensaje: " + httpClient.error);
                }
                else
                {
                    RecibirMensajes(listadoChat);
                }

                httpClient.Dispose();
            }
            }else {
                throw new Exception("No hay contenido en el Mensaje ");

            }
        }


    internal static IEnumerator RecibirMensajes(TextMeshProUGUI listadoChat)
    {
        Player player = FindObjectOfType<Player>();
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Chat/TodosMensajes", "GET");

        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.downloadHandler = new DownloadHandlerBuffer();

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("Problemas al recibir los mensajes : " + httpClient.error);
        }
        else
        {
            string jsonResponse = httpClient.downloadHandler.text;
            string response = "{\"myList\":" + jsonResponse + "}";
            ListaMensajes list = JsonUtility.FromJson<ListaMensajes>(response);

            //Obtener la grandaria de la lista de Mensajes
            int longitud = list.mylist.Count();

            foreach (MensajeJson o in list.mylist)
            {
                string identificador = o.IdPlayer;
                identificador.Substring(1, 3);
            
              listadoChat.text += "\n" + identificador + " > " + o.Mensaje + " .";


            }

            httpClient.Dispose();
        }

    }

    /// Dejar un metodo para ir recargando los mensajes cada par de segundos
    ///             yield return new WaitForSeconds(5);


    // Update is called once per frame
    void Update()
    {
        
    }
}
