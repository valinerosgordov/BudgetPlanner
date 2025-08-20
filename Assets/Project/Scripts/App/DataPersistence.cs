using System.IO;

using UnityEngine;

namespace FinancePlanner.Data
{
    public class DataPersistence : MonoBehaviour
    {
        [SerializeField] private DataStore data;
        [SerializeField] private string filename = "finance_data.json";

        string FullPath => Path.Combine(Application.persistentDataPath, filename);

        void Awake()
        {
            Load();
            if (data != null) data.OnChanged += Save; // автосейв на любое изменение
        }

        void OnDestroy()
        {
            if (data != null) data.OnChanged -= Save;
        }

        [ContextMenu("Save Now")]
        public void Save()
        {
            if (data == null) return;
            var dto = new TxListDTO
            {
                openingBalanceCents = data.OpeningBalanceCents,
                transactions = new System.Collections.Generic.List<Tx>(data.Transactions)
            };

            var json = JsonUtility.ToJson(dto, prettyPrint: true);
            Directory.CreateDirectory(Path.GetDirectoryName(FullPath));
            File.WriteAllText(FullPath, json);
#if UNITY_EDITOR
            Debug.Log($"[DataPersistence] Saved: {FullPath}");
#endif
        }

        [ContextMenu("Load Now")]
        public void Load()
        {
            if (data == null) return;
            if (!File.Exists(FullPath)) return;

            var json = File.ReadAllText(FullPath);
            var dto = JsonUtility.FromJson<TxListDTO>(json);
            if (dto == null) return;

            // Т.к. DataStore хранит список приватно, добавим простую «инициализацию» через AddTx
            // (или можно сделать метод Init в DataStore, если хочешь)
            var store = data;
            // очистка через рефлексию не нужна — просто создадим временный список и перезаполним:
            // сделаем это мягко: создадим новый ScriptableObject временно и скопируем
            // — проще: вызовем AddTx для каждой транзакции
            foreach (var tx in dto.transactions)
                store.AddTx(tx);
#if UNITY_EDITOR
            Debug.Log($"[DataPersistence] Loaded: {FullPath} ({dto.transactions.Count} tx)");
#endif
        }

        void OnApplicationQuit() => Save();
    }
}
