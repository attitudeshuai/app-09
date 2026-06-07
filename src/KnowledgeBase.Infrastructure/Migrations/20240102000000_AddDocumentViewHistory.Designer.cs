using System;
using KnowledgeBase.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KnowledgeBase.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240102000000_AddDocumentViewHistory")]
    partial class AddDocumentViewHistory
    {
    }
}
