# Hướng dẫn sử dụng cơ sở dữ liệu Hshop2023

## Lưu ý quan trọng

Trước khi thực hiện đoạn mã SQL dưới đây vào cơ sở dữ liệu, hãy nhớ **xóa đoạn mã sau**:

```sql
CONTAINMENT = NONE
ON PRIMARY 
( NAME = N'Hshop2023', FILENAME = N'E:\Sql\MSSQL16.SQLEXPRESS\MSSQL\DATA\Hshop2023.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
LOG ON 
( NAME = N'Hshop2023_log', FILENAME = N'E:\Sql\MSSQL16.SQLEXPRESS\MSSQL\DATA\Hshop2023_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [Hshop2023] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
BEGIN
    EXEC [Hshop2023].[dbo].[sp_fulltext_database] @action = 'enable'
END
