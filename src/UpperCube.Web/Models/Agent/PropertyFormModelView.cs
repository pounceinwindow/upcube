using Microsoft.AspNetCore.Mvc.Rendering;

namespace UpperCube.Web.Models.Agent;

public class PropertyFormModelView
{
        public string Title;
        public string Description;
        public decimal Price;
        public string Currency;
        public decimal Area;
        public int Rooms;
        public int Floor;
        public int TotalFloors;
        public int TransactionType;
        public int CityId;
        public int DistrictId;
        public int PropertyTypeId;
        public int CategoryId;
        public string Address;
        public IEnumerable<SelectListItem> Cities;
        public IEnumerable<SelectListItem> Districts;
        public IEnumerable<SelectListItem> PropertyTypes;
        public IEnumerable<SelectListItem> Categories;
}