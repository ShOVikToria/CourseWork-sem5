namespace ScanwordGenerator
{
    // Оновлений перелік типів клітинок
    public enum CellType
    {
        Empty,
        Block,
        Definition,
        Letter,
        Picture // <--- ЦЕЙ ТИП БУВ ВІДСУТНІЙ!
    }

    // Клас самої клітинки (без змін)
    public class Cell
    {
        public CellType Type { get; set; } = CellType.Empty;
        public char Letter { get; set; } = ' ';
        public string DefinitionText { get; set; }
        public string ArrowDirection { get; set; } // "->" або "v"

        // --- ДЛЯ КАРТИНОК ---
        public string ImagePath { get; set; }
        public int ImageWidthCells { get; set; }
        public int ImageHeightCells { get; set; }
        public bool IsPictureMainCell { get; set; }
    }
}