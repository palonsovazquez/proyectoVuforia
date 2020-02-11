using System;
using UnityEngine;
using UnityEngine.UI;
using Dummiesman;
using Parabox.Stl;
using UnityEngine.Networking;


// Include these namespaces to use BinaryFormatter
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

namespace GracesGames.SimpleFileBrowser.Scripts
{
    // Demo class to illustrate the usage of the FileBrowser script
    // Able to save and load files containing serialized data (e.g. text)
    public class testFile : MonoBehaviour
    {
        public bool debug = true;
        public InputField inputField;
        public GameObject dummyor;

        // Deslizador Para la escala
        public Slider deslizadorEscala;
        private float escala = 0.1f;

        public GameObject parentdummy;
        public GameObject loadedObject;
        public Text textoLog;

        // Use the file browser prefab
        public GameObject FileBrowserPrefab;

        // Define a file extension
        public string[] FileExtensions;






        //modo visualizacion del FileBrowser porttrail

        public bool PortraitMode;


        private string rutaBaseDescargas;


        private float escalaMinima = 0;
        private float escalaMaxima = 2;

        // Find the input field, label objects and add a onValueChanged listener to the input field
        private void Start()
        {



            // establece el valor original de la escala del objeto
            deslizadorEscala.SetValueWithoutNotify(0.12f);
            deslizadorEscala.minValue = escalaMinima;
            deslizadorEscala.maxValue = escalaMaxima;
            deslizadorEscala.onValueChanged.AddListener(delegate { setEscala(); });



            /*  
            Metodos para recuperar el intent de las clases de java y conseguir  el texto enviado por otra aplicacion con el SEND WITH
            */
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject context = jc.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject intent = context.Call<AndroidJavaObject>("getIntent");


            using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("android.os.Environment"))
            {
                rutaBaseDescargas = androidJavaClass.CallStatic<AndroidJavaObject>("getExternalStorageDirectory")
                    .Call<string>("getAbsolutePath");
                rutaBaseDescargas += "/ThingverseDownloads/";

            }
            textoLog.text += rutaBaseDescargas;






            try
            {
                string inputIntent = intent.Call<string>("getStringExtra", "android.intent.extra.TEXT"); //the key you used in the putExtra line on java

                if (inputIntent != null)
                {
                    inputIntent = inputIntent.Substring(inputIntent.IndexOf("https://"));
                    getThingverseZip(inputIntent);

                    inputField.text = inputIntent;
                }
            }
            catch (Exception e)
            {
                print("prueba exception: " + e);

            }



        }
        void Update()
        {



        }

        //metodo que cambia la escala del objeto cargado

        private void setEscala()
        {
            escala = deslizadorEscala.value;
            if (loadedObject != null)
            {
                loadedObject.transform.localScale = new Vector3(escala, escala, escala);
                
                // loadedObject.transform.position = parentdummy.transform.position;
            }


        }


        public void AbrirFileBrowser(string startPath)
        {
            bool saving = false;
            OpenFileBrowser(saving ? FileBrowserMode.Save : FileBrowserMode.Load, startPath);
        }

        private void AbrirFileBrowser(bool saving, string startPath)
        {
            OpenFileBrowser(saving ? FileBrowserMode.Save : FileBrowserMode.Load, startPath);
        }
        //Metodo generico para Cargar Ficheros Stl o obj
        public void Cargar(string path2)
        {
            if (path2.CompareTo("") == 0)
            {
                if (debug) textoLog.text += "error, ruta vacia\n";


            }
            else
            {
                if (loadedObject != null)
                {

                    Destroy(loadedObject);
                    if (debug) textoLog.text += "objeto destruido\n";
                }
                string[] texto = path2.Split('.');
                string extension = texto[(texto.Length - 1)];
                if (extension.ToLower().CompareTo("stl") == 0)
                {
                    loadedObject = CargarStl(path2);
                    if (debug) textoLog.text += " stl \n";



                }
                else if (extension.ToLower().CompareTo("obj") == 0)
                {
                    loadedObject = CargarObj(path2);
                    if (debug) textoLog.text += " obj \n";


                }
                loadedObject.transform.position = parentdummy.transform.position;
                loadedObject.transform.SetPositionAndRotation(parentdummy.transform.position,parentdummy.transform.rotation);
                // loadedObject.GetComponent<Renderer>().bounds.center;
                loadedObject.transform.SetParent(parentdummy.transform);


            }


        }


        // Metodo para cargar el fichero en formato STL


        private GameObject CargarStl(string ruta)
        {
            Mesh[] meshes = Importer.Import(ruta);
            loadedObject = new GameObject("dafda");
            for (int i = 0; i < meshes.Length; i++)
            {


                GameObject aaa = Instantiate(dummyor);

                aaa.GetComponent<MeshFilter>().mesh = meshes[i];
                aaa.transform.SetParent(loadedObject.transform);
                //Destroy(dummyor);
            }


            return loadedObject;




        }

        // Metodo para cargar el fichero en formato OBJ
        private GameObject CargarObj(string path2)
        {




            if (debug) textoLog.text += "objeto cargado\n";
            loadedObject = new OBJLoader().Load(path2);
            if (debug) textoLog.text += "objeto cargado2\n";

            return loadedObject;




        }

        // Open a file browser to save and load files
        private void OpenFileBrowser(FileBrowserMode fileBrowserMode, string startPath)
        {
            // Create the file browser and name it
            GameObject fileBrowserObject = Instantiate(FileBrowserPrefab, transform);
            fileBrowserObject.name = "FileBrowser";
            

            // Set the mode to save or load
            FileBrowser fileBrowserScript = fileBrowserObject.GetComponent<FileBrowser>();
            if (startPath == "")
            {
                fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape);
            }
            else
            {
                fileBrowserScript.SetupFileBrowser(PortraitMode ? ViewMode.Portrait : ViewMode.Landscape, startPath);
            }

            if (fileBrowserMode == FileBrowserMode.Load)
            {
                
                fileBrowserScript.OpenFilePanel(FileExtensions);
                // Subscribe to OnFileSelect event (call LoadFileUsingPath using path) 
                fileBrowserScript.OnFileSelect += LoadFileUsingPath;
            }
        }

        // Saves a file with the textToSave using a path


        // Loads a file using a path
        private void LoadFileUsingPath(string objPath)
        {
            if (debug) textoLog.text += objPath;
            if (!File.Exists(objPath))
            {
                if (debug) textoLog.text += "File doesn't exist.\n";
            }
            else
            {
                if (debug) textoLog.text += "File exist.\n";

                if (loadedObject != null)
                {

                    Destroy(loadedObject);

                    if (debug) textoLog.text += "objeto destruido\n";
                }

                Cargar(objPath);



            }
            Debug.Log("path=" + objPath);
            /*if (objPath.Length != 0)
            {

                BinaryFormatter bFormatter = new BinaryFormatter();
                // Open the file using the path
                FileStream file = File.OpenRead(objPath);
                // Convert the file from a byte array into a string
                string fileData = bFormatter.Deserialize(file) as string;
                // We're done working with the file so we can close it
                file.Close();
                // Set the LoadedText with the value of the file
                _loadedText.GetComponent<Text>().text = "Loaded data: \n" + fileData;
            }
            else
            {
                Debug.Log("Invalid path given");
            }*/
        }


        public void getThingverseZipButton()
        {
            getThingverseZip(inputField.text);



        }
        private string getThingverseZip(string url)
        {
            string ruta = "";
            string identificador = url.Split(':')[2];
            StartCoroutine(GetZip(url, identificador));

            // string newRuta = UnzipFile(ruta, identificador);




            return ruta;
        }


        IEnumerator GetZip(string url, string identificador)
        {

            //print(identificador);
            string ruta = rutaBaseDescargas + identificador + ".zip";


            if (!Directory.Exists(rutaBaseDescargas))
            {
                Directory.CreateDirectory(rutaBaseDescargas);
            }

            UnityWebRequest www = UnityWebRequest.Get(url + "/zip");
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                textoLog.text += www.error;
            }
            else
            {
                // Show results as text
                //Debug.Log(www.downloadHandler.text);

                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;

                textoLog.text += "\ndescarga terminada\n";
                System.IO.File.WriteAllBytes(ruta, results);
                string rutaFinal = UnzipFile(ruta, identificador);
                AbrirFileBrowser(false, rutaFinal);

            }

        }


        private string UnzipFile(string zipfilePath, string identificardor)
        {
            string exportLocation = "";
            if (zipfilePath != "")
            {



                string nombrefichero = "";
                string[] auxarray = zipfilePath.Split('/');
                string aux = auxarray[auxarray.Length - 1];
                print(aux);
                nombrefichero = aux.Remove(aux.Length - 4, 4);
                print("--" + nombrefichero);



                exportLocation = rutaBaseDescargas + nombrefichero + "-" + identificardor + "/";

                ZipUtil.Unzip(zipfilePath, exportLocation);

                textoLog.text += exportLocation + "\n";

            }
            return exportLocation + "files/";
        }

    }
}