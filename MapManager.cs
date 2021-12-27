using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapManager : MonoBehaviour
{
    //Информация об обьекте, используется для описания заготовок для спавна а также уже созданных обьектов на сцене
    [Serializable]
    public class GameObjectInfo
    {
        public GameObjectInfo() { }

        //Конструктор для упрощения создания обьекта
        public GameObjectInfo(string id, GameObject obj)
        {
            objectId = id;
            gameObject = obj;
        }

        //Строковый идентификатор обьекта для того чтобы сохранение понимало какой обьект создавать при загрузке
        public string objectId;
        //Сам обьект
        public GameObject gameObject;
    }

    //Класс-прокладка для сохранения данных, содержит в себе идентификатор обьекта и его координаты
    [System.Serializable]
    public class SaveData
    {
        public string prefabId;
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;

        public SaveData() { }

        //Конструктор для упрощения жизни
        public SaveData(GameObjectInfo info)
        {
            prefabId = info.objectId;

            position = info.gameObject.transform.position;
            eulerAngles = info.gameObject.transform.eulerAngles;
            scale = info.gameObject.transform.localScale;
        }
    }

    //то что я добавлял
    public GameObject SpawnPos;
    //public GameObject[] obje;
    //private GameObject gama;

    //дальше не я
    public string savePath;

    //Тут хранить список префабов, которые можно создавать в редакторе, обязательно указывать им уникальные идентификаторы
    public GameObjectInfo[] spawnObjects;

    //Тут будет храниться список уже созданных обьектов
    private List<GameObjectInfo> spawnedGameObjects = new List<GameObjectInfo>();

    private void Start()
    {
        savePath = Path.Combine(Application.dataPath, "stats.json");
    }

    //Функция для создания обьектов. ДЛЯ СОЗДАНИЯ ОБЬЕКТОВ С ВОЗМОЖНОСТЬЮ СОХРАНЕНИЯ. ИСПОЛЬЗОВАТЬ ЕЁ. ИНАЧЕ ОБЬЕКТ НЕ СОХРАНИТСЯ
    //objectId - идентификатор обьекта из списка spawnObjects, если не найдется, вернется null
    //Остальные параметры опциональны
    public GameObject CreateObject(string objectId, Vector3 position, Vector3 eulerAngles, Vector3 scale)
    {
        if (string.IsNullOrEmpty(objectId))
        {
            Debug.LogError("Can't spawn! Object Id is Empty or null!", this);
            return null;
        }

        //Проходим по нашему списку префабов
        for (int i = 0; i < spawnObjects.Length; i++)
        {
            //Если нашли наш идентификатор...
            if (spawnObjects[i].objectId == objectId)
            {
                //Создаем обьект по этому идентификатору
                GameObject spawnedObject = Instantiate(spawnObjects[i].gameObject, position, Quaternion.Euler(eulerAngles)) as GameObject;
                //Размер выставляется отдельно, т.к. неподдерживается в Instantiate
                spawnedObject.transform.localScale = scale;
                //Создаем описание созданного обьекта
                GameObjectInfo spawnedObjectInfo = new GameObjectInfo(objectId, spawnedObject);
                //И помещаем в список созданных обьектов
                spawnedGameObjects.Add(spawnedObjectInfo);
                //Возвращаем созданный обьект
                return spawnedObject;
            }
        }

        //Если не нашли обьект с указанным идентификатором, выкидываем ошибку и возвращем null
        Debug.LogError($"Can't find object with id {objectId}!");
        return null;
    }

    public GameObject CreateObject(string objectId)
    {
        return CreateObject(objectId, Vector3.zero, Vector3.zero, Vector3.one);
    }

    //Функция для удаления созданных обьектов
    //obj - созданный обьект
    public void DestroyObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("Can't Destroy null object!", this);
            return;
        }

        for (int i = 0; i < spawnedGameObjects.Count; i++)
        {
            if (spawnedGameObjects[i].gameObject == obj)
            {
                Destroy(obj);
                spawnedGameObjects.RemoveAt(i);
                return;
            }
        }

        //Если не нашли такой обьект, выбиваем ошибку
        Debug.LogError($"Can't find object with name {obj.name} in save list!", obj);
    }

    //сохранение
    public void Save()
    {
        using (StreamWriter writer = new StreamWriter(File.OpenWrite(savePath)))
        {
            for (int i = 0; i < spawnedGameObjects.Count; i++)
            {
                SaveData objData = new SaveData(spawnedGameObjects[i]);

                writer.WriteLine(JsonUtility.ToJson(objData));
            }
        }
    }

    //загрузка
    public void Load()
    {
        if (File.Exists(savePath))
        {
            string[] jsonData = File.ReadAllLines(savePath);

            for (int i = 0; i < jsonData.Length; i++)
            {
                var saveData = JsonUtility.FromJson<SaveData>(jsonData[i]);

                CreateObject(saveData.prefabId, saveData.position, saveData.eulerAngles, saveData.scale);
            }
        }
    }


    //создание обьектов, это уже от меня, работает само по себе, но надо ещё добавить синхронизацию с его сохраением в json
    //public void SpawnObjects(int NomerObject)
    //{
    //    Instantiate(obje[NomerObject], SpawnPos.transform.position, Quaternion.identity);
    //}

    public void Crit(string objectId)
    {
        CreateObject(objectId, Vector3.zero, Vector3.zero, Vector3.one);
    }
}