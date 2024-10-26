
namespace YG
{
    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;


        // saves
        public bool[] unlockLevels = new bool[31]; // level number verilecek.
        public int[] levelsStarCount = new int[31];
        public bool isMusicOn = true;


        // Вы можете выполнить какие то действия при загрузке сохранений

        public SavesYG()
        {
            // Допустим, задать значения по умолчанию для отдельных элементов массива

            unlockLevels[1] = true;
        }
    }
}
