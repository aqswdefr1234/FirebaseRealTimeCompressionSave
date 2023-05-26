using System.Collections.Generic;//����Ʈ
using System.Collections;//�ڷ�ƾ
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Database;

public class TestRealTimedDatabase : MonoBehaviour//https://papabee.tistory.com/337
{
    [SerializeField] private Transform parentPrefab;
    [SerializeField] private Transform childPrefab;
    [SerializeField] private TMP_Text testText;
    [SerializeField] private Transform objectsSpace;

    private string url = "YourURL";
    private string dataFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/DataFolder/OBJ_File";
    private string sceneDataPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/DataFolder/SceneData";
    private DatabaseReference myDatabaseRef;
    private string sceneDataJson = "";
    private TransformData tfd;//���� �ִ� ������Ʈ���� ������ ��� �ִ�.
    private List<OBJ_DataCustomParsing> objData = new List<OBJ_DataCustomParsing>();

    private Transform parentPrefabClone;
    private Transform childPrefabClone;
    private string newPath;

    void Start()
    {
        sceneDataJson = File.ReadAllText(sceneDataPath);
        tfd = JsonUtility.FromJson<TransformData>(sceneDataJson);
        myDatabaseRef = FirebaseDatabase.GetInstance(url).RootReference;
    }
    void SaveObjectTest()
    {
        for (int i = 0; i < tfd.myName.Count; i++)
        {
            myDatabaseRef.Child("Kim").Child("ObjectsData").Child(Path.GetFileName(tfd.path[i])).SetValueAsync(SaveCompressedJsonToFile(File.ReadAllText(tfd.path[i])));
        }
    }
    public void ReadOjbectsDataJson()
    {
        string zipData = "";
        string jsonData = "";
        myDatabaseRef.Child("Kim").Child("ObjectsData").GetValueAsync().ContinueWith(task =>
        {//Convert.FromBase64String
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    zipData = (string)childSnapshot.Value;
                    jsonData = LoadCompressedJsonFromFile(zipData);
                    objData.Add(JsonUtility.FromJson<OBJ_DataCustomParsing>(jsonData));
                    
                }
                Debug.Log("objData : " + objData.Count);
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to read data: " + task.Exception);
            }
        });
    }
    private void ReadRoomDataJson()
    {
        myDatabaseRef.Child("Kim").Child("RoomData").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string jsonData = snapshot.GetRawJsonValue();
            }
            else if (task.IsFaulted)
            {
                Debug.LogError("Failed to read data: " + task.Exception);
            }
        });
    }
    IEnumerator LoadRoomData()
    {
        WaitForSeconds delay = new WaitForSeconds(0.1f);

        yield return delay;
        myDatabaseRef.Child("Kim").Child("RoomData").SetRawJsonValueAsync(sceneDataJson);
        yield return delay;
        SaveObjectTest();
    }
    public void TestButton()
    {
        try
        {
            StartCoroutine(LoadRoomData());
            testText.text = "success";
        }
        catch (Exception e)
        {
            testText.text = e.ToString();
        }
    }
    public void RemoveData()//Ű ������ �����ϰ������ SetValueAsync(null)�� ���
    {
        myDatabaseRef.SetValueAsync(null);
    }
    private string SaveCompressedJsonToFile(string jsonData)
    {
        string zipData = "";
        byte[] compressedData;
        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                using (StreamWriter writer = new StreamWriter(gzipStream))
                {
                    writer.Write(jsonData);
                }
            }
            compressedData = memoryStream.ToArray();//����� byte[] ������
        }
        zipData = Convert.ToBase64String(compressedData);////����� byte[] �����͸� string���� ���ڵ�
        Debug.Log("Compressed JSON Data saved to file");
        return zipData;
        
    }

    private string LoadCompressedJsonFromFile(string zipData)
    {
        byte[] compressedData = Convert.FromBase64String(zipData);

        using (MemoryStream memoryStream = new MemoryStream(compressedData))
        {
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                using (StreamReader reader = new StreamReader(gzipStream))
                {
                    string jsonData = reader.ReadToEnd();
                    return jsonData;
                }
            }
        }
    }
}