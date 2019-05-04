﻿using Microsoft.AspNetCore.Razor.TagHelpers;
using System.ComponentModel;
using System.Text;

namespace AspNetCoreServerSide.TagHelpers
{
    [HtmlTargetElement("datatables",Attributes = "id,class,model")]
    public class DataTablesTagHelper:TagHelper
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public object Model { get; set; }

        [HtmlAttributeName("thead-class")]
        public string TheadClass { get; set; }

        [HtmlAttributeName("search-row-th-class")]
        public string SearchRowThClass { get; set; }

        [HtmlAttributeName("search-input-class")]
        public string SearchInputClass { get; set; }

        public override void Process(TagHelperContext context,TagHelperOutput output)
        {
            output.TagName = "table";
            output.Attributes.Add("id",Id);
            output.Attributes.Add("class",Class);

            output.PreContent.SetHtmlContent($@"<thead class=""{TheadClass}"">");

            var properties = TypeDescriptor.GetProperties(Model.GetType());

            var headerRow = new StringBuilder();
            var searchRow = new StringBuilder();

            headerRow.AppendLine("<tr>");
            searchRow.AppendLine("<tr>");

            foreach(PropertyDescriptor prop in properties)
            {
                var column = prop.DisplayName ?? prop.Name;

                headerRow.AppendLine($"<th>{column}</th>");
                searchRow.AppendLine($@"<th class=""{SearchRowThClass}""><span class=""sr-only"">{column}</span><input type=""search"" class=""{SearchInputClass}"" placeholder=""Search {column}"" aria-label=""{column}"" /></th>");
            }

            headerRow.AppendLine("</tr>");
            searchRow.AppendLine("</tr>");

            output.Content.SetHtmlContent($"{headerRow.ToString()}{searchRow.ToString()}");
            output.PostContent.SetHtmlContent("</thead>");
        }
    }
}
