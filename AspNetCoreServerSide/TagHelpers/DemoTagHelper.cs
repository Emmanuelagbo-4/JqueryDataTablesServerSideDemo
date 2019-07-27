﻿using JqueryDataTables.ServerSide.AspNetCoreWeb.Attributes;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AspNetCoreServerSide.TagHelpers
{
    [HtmlTargetElement("demo", Attributes = "id,class,model")]
    public class DemoTagHelper : TagHelper
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public object Model { get; set; }

        [HtmlAttributeName("enable-searching")]
        public bool EnableSearching { get; set; }

        [HtmlAttributeName("thead-class")]
        public string TheadClass { get; set; }

        [HtmlAttributeName("search-row-th-class")]
        public string SearchRowThClass { get; set; }

        [HtmlAttributeName("search-input-class")]
        public string SearchInputClass { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";
            output.Attributes.Add("id", Id);
            output.Attributes.Add("class", Class);

            output.PreContent.SetHtmlContent($@"<thead class=""{TheadClass}"">");

            var headerRow = new StringBuilder();
            var searchRow = new StringBuilder();

            headerRow.AppendLine("<tr>");

            if (EnableSearching)
            {
                searchRow.AppendLine("<tr>");
            }

            var columns = GetColumnsFromModel(Model.GetType());

            foreach (var column in columns)
            {
                headerRow.AppendLine($"<th>{column}</th>");

                if (!EnableSearching)
                {
                    continue;
                }

                searchRow.AppendLine($@"<th class=""{SearchRowThClass}""><span class=""sr-only"">{column}</span><input type=""search"" class=""{SearchInputClass}"" placeholder=""Search {column}"" aria-label=""{column}"" /></th>");
            }

            headerRow.AppendLine("</tr>");
            if (EnableSearching)
            {
                searchRow.AppendLine("</tr>");
            }

            output.Content.SetHtmlContent($"{headerRow.ToString()}{searchRow.ToString()}");
            output.PostContent.SetHtmlContent("</thead>");
        }

        private static IEnumerable<string> GetColumnsFromModel(Type parentClass)
        {
            var complexProperties = parentClass.GetTypeInfo()
                       .DeclaredProperties
                       .Where(p => p.GetCustomAttributes<NestedSortableAttribute>().Any() || p.GetCustomAttributes<NestedSearchableAttribute>().Any());

            var properties = parentClass.GetTypeInfo()
                       .DeclaredProperties
                       .Where(p => p.GetCustomAttributes<SortableAttribute>().Any() || p.GetCustomAttributes<SearchableAttribute>().Any()); ;

            foreach (var prop in properties.Except(complexProperties))
            {
                var propertyDescriptor = GetPropertyDescriptor(prop);

                yield return propertyDescriptor.DisplayName ?? propertyDescriptor.Name;
            }

            if (complexProperties.Any())
            {
                foreach (var parentProperty in complexProperties)
                {
                    var parentType = parentProperty.PropertyType;

                    var nestedProperties = GetColumnsFromModel(parentType);

                    foreach (var nestedProperty in nestedProperties)
                    {
                        yield return nestedProperty;
                    }
                }
            }
        }

        public static PropertyDescriptor GetPropertyDescriptor(PropertyInfo propertyInfo)
        {
            return TypeDescriptor.GetProperties(propertyInfo.DeclaringType).Find(propertyInfo.Name, false);
        }
    }
}
