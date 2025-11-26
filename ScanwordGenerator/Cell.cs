namespace ScanwordGenerator
{
    // Перелічення типів клітинок
    public enum CellType
    {
        Empty,      // Порожнє місце
        Block,      // Заблокована
        Definition, // Питання
        Letter      // Літера
    }

    // Клас самої клітинки
    public class Cell
    {
        public CellType Type { get; set; } = CellType.Empty;
        public char Letter { get; set; } = ' ';
        public string DefinitionText { get; set; }
        public string ArrowDirection { get; set; } // "->" або "v"
    }
}