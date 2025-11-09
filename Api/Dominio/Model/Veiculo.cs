using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MinimalApi.Dominio.Models;

public class Veiculo
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Marca { get; set; } = default!;

    [Required]
    [StringLength(50)]
    public string Modelo { get; set; } = default!;
    public int Ano { get; set; }
}