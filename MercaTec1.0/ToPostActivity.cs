using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.AppCompat.App;
using System.IO;
using Plugin.CurrentActivity;
using Android.Graphics;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Plugin.Media;

namespace MercaTec1._0
{
    [Activity(Label = "ToPostActivity")]
    public class ToPostActivity : Activity
    {
        string Archivo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.ToPostActivity);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            var imagen = FindViewById<ImageView>(Resource.Id.iProducto);
            var txtNombreProducto = FindViewById<EditText>(Resource.Id.txtNombreProducto);
            var txtDetalleProducto = FindViewById<EditText>(Resource.Id.txtDetallesProducto);
            var txtCondicion = FindViewById<EditText>(Resource.Id.txtCondicion);
            var txtNombreVendedor = FindViewById<EditText>(Resource.Id.txtNombreVendedor);
            var txtPrecio = FindViewById<EditText>(Resource.Id.txtPrecioProducto);
            var txtUnidades = FindViewById<EditText>(Resource.Id.txtUnidades);
            var txtEstatus = FindViewById<EditText>(Resource.Id.txtEstatus);
            var txtContacto = FindViewById<EditText>(Resource.Id.txtContacto);
            var btnPublicar = FindViewById<Button>(Resource.Id.btnPublicar);
            var btnSalir = FindViewById<Button>(Resource.Id.btnSalir);

            imagen.Click += async delegate
            {
                await CrossMedia.Current.Initialize();
                var archivo = await CrossMedia.Current.TakePhotoAsync(
                    new Plugin.Media.Abstractions.StoreCameraMediaOptions
                    {
                        Directory = "imagenes",
                        Name = txtNombreProducto.Text,
                        CompressionQuality = 30,
                        CustomPhotoSize = 30,
                        PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                        DefaultCamera = Plugin.Media.Abstractions.CameraDevice.Rear
                    });
                if (archivo == null)
                    return;
                Bitmap bp = BitmapFactory.DecodeStream(archivo.GetStream());
                Archivo = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), txtNombreProducto.Text + ".jpg");
                var stream = new FileStream(Archivo, FileMode.Create);
                bp.Compress(Bitmap.CompressFormat.Jpeg, 30, stream);
                imagen.SetImageBitmap(bp);
            };
            btnPublicar.Click += async delegate
            {
                if(txtNombreProducto.Text.Equals(string.Empty) || txtDetalleProducto.Text.Equals(string.Empty) || txtCondicion.Text.Equals(string.Empty) || txtNombreVendedor.Text.Equals(string.Empty) 
                || txtPrecio.Text.Equals(string.Empty) || txtUnidades.Text.Equals(string.Empty) || txtEstatus.Equals(string.Empty) || txtContacto.Text.Equals(string.Empty))
                    Toast.MakeText(this, "Tienes campos vacios, completalos", ToastLength.Long).Show();
                else
                {
                    try
                    {
                        var CuentaAlmacenamiento = CloudStorageAccount.Parse
                            ("LINK CUENTA ALMACENAMIENTO");
                        var ClienteBlob = CuentaAlmacenamiento.CreateCloudBlobClient();
                        var Carpeta = ClienteBlob.GetContainerReference("imagenes");
                        var resourceBlob = Carpeta.GetBlockBlobReference(txtNombreProducto.Text + ".jpg");
                        resourceBlob.Properties.ContentType = "image/jpeg";
                        await resourceBlob.UploadFromFileAsync(Archivo.ToString());
                        var TablaNoSQl = CuentaAlmacenamiento.CreateCloudTableClient();
                        var Coleccion = TablaNoSQl.GetTableReference("MercaTec");
                        await Coleccion.CreateIfNotExistsAsync();
                        var venta = new Ventas("Merca-Tec", txtNombreProducto.Text);
                        venta.Condicion = txtCondicion.Text;
                        venta.Contacto = txtContacto.Text;
                        venta.DetallesProducto = txtDetalleProducto.Text;
                        venta.Estatus = txtEstatus.Text;
                        venta.Imagen = "LINK PARA IMAGEN" + txtNombreProducto.Text + ".jpg";
                        venta.NombreVendedor = txtNombreVendedor.Text;
                        venta.PrecioProducto = double.Parse(txtPrecio.Text);
                        venta.Unidades = int.Parse(txtUnidades.Text);
                        var Store = TableOperation.Insert(venta);
                        await Coleccion.ExecuteAsync(Store);
                        Toast.MakeText(this, "Producto publicado correctamente", ToastLength.Long).Show();
                        StartActivity(typeof(MainActivity));
                        Finish();
                    }
                    catch (Exception ex)
                    {
                        Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                    }
                }
            };

            btnSalir.Click += delegate
            {
                Finish();
                StartActivity(typeof(MainActivity));
            };
        }
    }

    public class Ventas : TableEntity
    {
        public Ventas(string Categoria, string NombreProducto)
        {
            PartitionKey = Categoria;
            RowKey = NombreProducto;
        }
        public string Condicion { get; set; }
        public string Contacto { get; set; }
        public string DetallesProducto { get; set; }
        public string Estatus { get; set; }
        public string Imagen { get; set; }
        public string NombreVendedor { get; set; }
        public double PrecioProducto { get; set; }
        public int Unidades { get; set; }

    }
}