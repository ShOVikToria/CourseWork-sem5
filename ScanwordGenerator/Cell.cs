namespace ScanwordGenerator
{
    public enum CellType
    {
        Empty,
        Block,
        Definition,
        Letter,
        Picture
    }

    public class Cell
    {
        public CellType Type { get; set; } = CellType.Empty;
        public char Letter { get; set; } = ' ';
        public string DefinitionText { get; set; }
        public string ArrowDirection { get; set; } // "->" або "v"

        public string ImagePath { get; set; }
        public int ImageWidthCells { get; set; }
        public int ImageHeightCells { get; set; }
        public bool IsPictureMainCell { get; set; }

        // НОВЕ ПОЛЕ: Зміщення стрілки відносно початку картинки
        // 0 = перший рядок, 1 = другий і т.д.
        public int ArrowOffset { get; set; }
    }
}