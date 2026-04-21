using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Multiplatoform_Project.Models;

namespace Multiplatoform_Project.Services
{
    public class FirebaseAuthServices
    {
        private const string ApiKey = "Your api key";
        private const string ProjectId = "your project id";
        private const string StorageBucket = "your storage bucket";

        private const string AuthBaseUrl = "https://identitytoolkit.googleapis.com/v1/accounts";
        private const string FirestoreBase = "https://firestore.googleapis.com/v1/projects/{0}/databases/(default)/documents";
        private const string StorageBase = "https://firebasestorage.googleapis.com/v0/b/{0}/o";

        private static FirebaseAuthServices? _instance;
        public static FirebaseAuthServices Instance => _instance ??= new FirebaseAuthServices();

        private readonly HttpClient _httpClient;

        public string? IdToken { get; private set; }
        public string? CurrentUserId { get; private set; }

        public FirebaseAuthServices()
        {
            _httpClient = new HttpClient();
        }

       
        // PRIVATE HELPER — sets Bearer token on every Firestore request
        

        
        private string AuthUrl(string url)
        {
            if (!string.IsNullOrEmpty(IdToken))
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", IdToken);
            else
                _httpClient.DefaultRequestHeaders.Authorization = null;

            return url; 
        }

        
        // AUTH
        

        public async Task SignUpAsync(string firstName, string lastName, string email, string password)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var url = $"{AuthBaseUrl}:signUp?key={ApiKey}";
            var data = new { email, password, returnSecureToken = true };

            var response = await _httpClient.PostAsJsonAsync(url, data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(ParseFirebaseError(content));

            var json = JsonDocument.Parse(content).RootElement;
            IdToken = json.GetProperty("idToken").GetString();
            CurrentUserId = json.GetProperty("localId").GetString();

            var profile = new UserProfile
            {
                UserId = CurrentUserId!,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = "",
                ShippingAddress = "",
                MemberSince = DateTime.UtcNow,
                IsAdmin = false
            };

            await CreateUserProfileAsync(profile);
        }

        public async Task SignInAsync(string email, string password)
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var url = $"{AuthBaseUrl}:signInWithPassword?key={ApiKey}";
            var data = new { email, password, returnSecureToken = true };

            var response = await _httpClient.PostAsJsonAsync(url, data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(ParseFirebaseError(content));

            var json = JsonDocument.Parse(content).RootElement;
            IdToken = json.GetProperty("idToken").GetString();
            CurrentUserId = json.GetProperty("localId").GetString();
        }

        public async Task SendPasswordResetAsync(string email)
        {
            var url = $"{AuthBaseUrl}:sendOobCode?key={ApiKey}";
            var data = new { requestType = "PASSWORD_RESET", email };

            var response = await _httpClient.PostAsJsonAsync(url, data);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(ParseFirebaseError(content));
        }

        public void SignOut()
        {
            IdToken = null;
            CurrentUserId = null;
            // Clear the auth header so no further requests are authenticated
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        
        // USERS
        

        public async Task CreateUserProfileAsync(UserProfile profile)
        {
            var url = DocUrl($"users/{profile.UserId}");
            var body = WrapFields(new Dictionary<string, object>
            {
                ["firstName"] = profile.FirstName,
                ["lastName"] = profile.LastName,
                ["email"] = profile.Email,
                ["phone"] = profile.Phone,
                ["shippingAddress"] = profile.ShippingAddress,
                ["memberSince"] = profile.MemberSince.ToString("O"),
                ["isAdmin"] = profile.IsAdmin
            });
            await PatchAsync(url, body);
        }

        public async Task<UserProfile?> GetUserProfileAsync(string userId)
        {
            var response = await _httpClient.GetAsync(AuthUrl(DocUrl($"users/{userId}")));
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;

            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception(ParseFirebaseError(content));

            var fields = JsonDocument.Parse(content).RootElement.GetProperty("fields");
            return new UserProfile
            {
                UserId = userId,
                FirstName = Str(fields, "firstName"),
                LastName = Str(fields, "lastName"),
                Email = Str(fields, "email"),
                Phone = Str(fields, "phone"),
                ShippingAddress = Str(fields, "shippingAddress"),
                MemberSince = DateTime.TryParse(Str(fields, "memberSince"), out var dt) ? dt : DateTime.UtcNow,
                IsAdmin = Bool(fields, "isAdmin")
            };
        }

        public async Task UpdateUserProfileAsync(UserProfile profile)
        {
            var fields = new Dictionary<string, object>
            {
                ["firstName"] = profile.FirstName,
                ["lastName"] = profile.LastName,
                ["email"] = profile.Email,
                ["phone"] = profile.Phone,
                ["shippingAddress"] = profile.ShippingAddress
            };
            var url = DocUrl($"users/{profile.UserId}") + BuildMask(fields.Keys);
            await PatchAsync(url, WrapFields(fields));
        }

       
        // CATEGORIES
      

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var response = await _httpClient.GetAsync(AuthUrl(DocUrl("categories")));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(content));

            var root = JsonDocument.Parse(content).RootElement;
            if (!root.TryGetProperty("documents", out var docs)) return new();

            return docs.EnumerateArray().Select(d =>
            {
                var f = d.GetProperty("fields");
                return new Category
                {
                    Id = DocId(d),
                    Name = Str(f, "name"),
                    IconName = Str(f, "iconName"),
                    ProductCount = Int(f, "productCount")
                };
            }).ToList();
        }

        public async Task AddCategoryAsync(Category cat)
        {
            var body = WrapFields(new Dictionary<string, object>
            {
                ["name"] = cat.Name,
                ["iconName"] = cat.IconName,
                ["productCount"] = cat.ProductCount
            });
            await PostFirestoreAsync(DocUrl("categories"), body);
        }

        public async Task UpdateCategoryAsync(Category cat)
        {
            var fields = new Dictionary<string, object>
            {
                ["name"] = cat.Name,
                ["iconName"] = cat.IconName,
                ["productCount"] = cat.ProductCount
            };
            var url = DocUrl($"categories/{cat.Id}") + BuildMask(fields.Keys);
            await PatchAsync(url, WrapFields(fields));
        }

        public async Task DeleteCategoryAsync(string categoryId)
        {
            var response = await _httpClient.DeleteAsync(AuthUrl(DocUrl($"categories/{categoryId}")));
            response.EnsureSuccessStatusCode();
        }

        
        // PRODUCTS
        

        public async Task<List<Product>> GetProductsByCategoryAsync(string categoryId)
        {
            var docs = await RunQueryAsync(new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "products" } },
                    where = new
                    {
                        compositeFilter = new
                        {
                            op = "AND",
                            filters = new object[]
                            {
                                MakeFilter("categoryId", "EQUAL", categoryId),
                                MakeFilter("available",  "EQUAL", true)
                            }
                        }
                    }
                }
            });
            return docs.Select(MapProduct).ToList();
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var response = await _httpClient.GetAsync(AuthUrl(DocUrl("products")));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(content));

            var root = JsonDocument.Parse(content).RootElement;
            if (!root.TryGetProperty("documents", out var docs)) return new();
            return docs.EnumerateArray().Select(MapProduct).ToList();
        }

        public async Task<Product?> GetProductAsync(string productId)
        {
            var response = await _httpClient.GetAsync(AuthUrl(DocUrl($"products/{productId}")));
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(content));
            return MapProduct(JsonDocument.Parse(content).RootElement);
        }

        public async Task<string> AddProductAsync(Product product)
        {
            var doc = await PostFirestoreAsync(DocUrl("products"), WrapFields(ProductDict(product)));
            return DocId(doc);
        }

        public async Task UpdateProductAsync(Product product)
        {
            var fields = ProductDict(product);
            var url = DocUrl($"products/{product.Id}") + BuildMask(fields.Keys);
            await PatchAsync(url, WrapFields(fields));
        }

        public async Task DeleteProductAsync(string productId)
        {
            var response = await _httpClient.DeleteAsync(AuthUrl(DocUrl($"products/{productId}")));
            response.EnsureSuccessStatusCode();
        }

        
        // ORDERS
        

        public async Task<string> PlaceOrderAsync(Order order)
        {
            var itemValues = order.Items.Select(i => (object)new
            {
                mapValue = new
                {
                    fields = new Dictionary<string, object>
                    {
                        ["productId"] = new { stringValue = i.ProductId },
                        ["name"] = new { stringValue = i.Name },
                        ["imageUrl"] = new { stringValue = i.ImageUrl },
                        ["price"] = new { doubleValue = i.Price },
                        ["quantity"] = new { integerValue = i.Quantity.ToString() },
                        ["variant"] = new { stringValue = i.Variant },
                       
                    }
                }
            }).ToList();

            var fields = new Dictionary<string, object>
            {
                ["userId"] = new { stringValue = order.UserId },
                ["customerName"] = new { stringValue = order.CustomerName },
                ["date"] = new { timestampValue = order.Date.ToUniversalTime().ToString("O") },
                ["status"] = new { stringValue = order.Status },
                ["total"] = new { doubleValue = order.Total },
                ["paymentMethod"] = new { stringValue = order.PaymentMethod },
                ["paymentId"] = new { stringValue = order.PaymentId ?? "" },
                ["shippingAddress"] = new { stringValue = order.ShippingAddress },
                ["items"] = new { arrayValue = new { values = itemValues } }
            };


            string body = JsonSerializer.Serialize(new { fields });
            var doc = await PostFirestoreAsync(DocUrl("orders"), body);
            string docId = DocId(doc);

            string friendlyId = $"ORD-{docId[..4].ToUpper()}";
            var idFields = new Dictionary<string, object> { ["orderId"] = friendlyId };
            await PatchAsync(DocUrl($"orders/{docId}") + BuildMask(idFields.Keys), WrapFields(idFields));

            return friendlyId;
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            var response = await _httpClient.GetAsync(AuthUrl(DocUrl("orders")));
            var content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(content));

            var root = JsonDocument.Parse(content).RootElement;
            if (!root.TryGetProperty("documents", out var docs)) return new();

            return docs.EnumerateArray()
                       .Select(MapOrder)
                       .Where(o => o.UserId == userId)
                       .OrderByDescending(o => o.Date)
                       .ToList();
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var docs = await RunQueryAsync(new
            {
                structuredQuery = new
                {
                    from = new[] { new { collectionId = "orders" } },
                    orderBy = new[] { new { field = new { fieldPath = "date" }, direction = "DESCENDING" } }
                }
            });
            return docs.Select(MapOrder).ToList();
        }

        public async Task UpdateOrderStatusAsync(string orderId, string status)
        {
            var fields = new Dictionary<string, object> { ["status"] = status };
            await PatchAsync(DocUrl($"orders/{orderId}") + BuildMask(fields.Keys), WrapFields(fields));
        }

        
        // STORAGE
        

        public async Task<string> UploadProductImageAsync(Stream imageStream, string fileName)
        {
            string encodedName = Uri.EscapeDataString($"products/{fileName}");
            string url = string.Format(StorageBase, StorageBucket) + $"?name={encodedName}";

            var content = new StreamContent(imageStream);
            content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            // Storage still uses Bearer header (correct)
            _httpClient.DefaultRequestHeaders.Authorization =
                string.IsNullOrEmpty(IdToken) ? null :
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", IdToken);

            var response = await _httpClient.PostAsync(url, content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(body));

            var token = JsonDocument.Parse(body).RootElement
                .GetProperty("downloadTokens").GetString()!;

            return $"https://firebasestorage.googleapis.com/v0/b/{StorageBucket}/o/{encodedName}?alt=media&token={token}";
        }

        
        // PRIVATE HELPERS

        private string DocUrl(string path)
            => string.Format(FirestoreBase, ProjectId) + $"/{path}";

        private static string BuildMask(IEnumerable<string> keys)
            => "?" + string.Join("&", keys.Select(k => $"updateMask.fieldPaths={k}"));

        private async Task<JsonElement> PostFirestoreAsync(string url, string body)
        {
            // AuthUrl sets the Bearer header and returns the clean URL
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(AuthUrl(url), content);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(result));
            return JsonDocument.Parse(result).RootElement;
        }

        private async Task<JsonElement> PostFirestoreAsync(string url, object payload)
            => await PostFirestoreAsync(url, JsonSerializer.Serialize(payload));

        private async Task PatchAsync(string url, string body)
        {
            var request = new HttpRequestMessage(HttpMethod.Patch, AuthUrl(url))
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(result));
        }

        private async Task<List<JsonElement>> RunQueryAsync(object query)
        {
            string queryUrl = $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents:runQuery";
            var body = JsonSerializer.Serialize(query);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(AuthUrl(queryUrl), content);
            var result = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) throw new Exception(ParseFirebaseError(result));

            var list = new List<JsonElement>();
            foreach (var item in JsonDocument.Parse(result).RootElement.EnumerateArray())
                if (item.TryGetProperty("document", out var doc))
                    list.Add(doc);
            return list;
        }

        private static string WrapFields(Dictionary<string, object> data)
        {
            var fields = new Dictionary<string, object>();
            foreach (var kv in data)
                fields[kv.Key] = kv.Value switch
                {
                    string s => new { stringValue = s },
                    bool b => new { booleanValue = b },
                    int i => new { integerValue = i.ToString() },
                    double d => new { doubleValue = d },
                    _ => new { stringValue = kv.Value?.ToString() ?? "" }
                };
            return JsonSerializer.Serialize(new { fields });
        }

        private static object MakeFilter(string field, string op, object value)
        {
            object fv = value switch
            {
                string s => new { stringValue = s },
                bool b => new { booleanValue = b },
                int i => new { integerValue = i.ToString() },
                double d => new { doubleValue = d },
                _ => new { stringValue = value.ToString()! }
            };
            return new { fieldFilter = new { field = new { fieldPath = field }, op, value = fv } };
        }

        private static Dictionary<string, object> ProductDict(Product p) => new()
        {
            ["name"] = p.Name,
            ["categoryId"] = p.categoryId,
            ["categoryName"] = p.categoryName,
            ["price"] = p.Price,
            ["description"] = p.Description,
            ["imageUrl"] = p.imageUrl,
            ["badge"] = p.badge,
            ["available"] = p.available
        };

        private static string DocId(JsonElement doc)
            => doc.GetProperty("name").GetString()!.Split('/').Last();

        private static string Str(JsonElement f, string key)
        {
            if (!f.TryGetProperty(key, out var v)) return "";
            if (v.TryGetProperty("stringValue", out var s)) return s.GetString() ?? "";
            if (v.TryGetProperty("timestampValue", out var t)) return t.GetString() ?? "";
            return "";
        }

        private static bool Bool(JsonElement f, string key)
        {
            if (!f.TryGetProperty(key, out var v)) return false;
            return v.TryGetProperty("booleanValue", out var b) && b.GetBoolean();
        }

        private static int Int(JsonElement f, string key)
        {
            if (!f.TryGetProperty(key, out var v)) return 0;
            if (v.TryGetProperty("integerValue", out var i) && int.TryParse(i.GetString(), out int r)) return r;
            if (v.TryGetProperty("doubleValue", out var d)) return (int)d.GetDouble();
            return 0;
        }

        private static double Dbl(JsonElement f, string key)
        {
            if (!f.TryGetProperty(key, out var v)) return 0;
            if (v.TryGetProperty("doubleValue", out var d)) return d.GetDouble();
            if (v.TryGetProperty("integerValue", out var i) && double.TryParse(i.GetString(), out double r)) return r;
            return 0;
        }

        private static Product MapProduct(JsonElement doc)
        {
            var f = doc.GetProperty("fields");
            return new Product
            {
                Id = DocId(doc),
                Name = Str(f, "name"),
                categoryId = Str(f, "categoryId"),
                categoryName = Str(f, "categoryName"),
                Price = Dbl(f, "price"),
                Description = Str(f, "description"),
                imageUrl = Str(f, "imageUrl"),
                badge = Str(f, "badge"),
                available = Bool(f, "available")
            };
        }

        private static Order MapOrder(JsonElement doc)
        {
            var f = doc.GetProperty("fields");
            var items = new List<OrderItem>();

            if (f.TryGetProperty("items", out var ip) &&
                ip.TryGetProperty("arrayValue", out var av) &&
                av.TryGetProperty("values", out var vals))
            {
                foreach (var v in vals.EnumerateArray())
                {
                    var mf = v.GetProperty("mapValue").GetProperty("fields");
                    items.Add(new OrderItem
                    {
                        ProductId = Str(mf, "productId"),
                        Name = Str(mf, "name"),
                        ImageUrl = Str(mf, "imageUrl"),
                        Price = Dbl(mf, "price"),
                        Quantity = Int(mf, "quantity"),
                        Variant = Str(mf, "variant"),
                        PaymentId = Str(f,"paymentId")
                         
                    });
                }
            }

            return new Order
            {
                DocId = DocId(doc),
                OrderId = Str(f, "orderId"),
                UserId = Str(f, "userId"),
                CustomerName = Str(f, "customerName"),
                Date = DateTime.TryParse(Str(f, "date"), out var dt) ? dt : DateTime.UtcNow,
                Status = Str(f, "status"),
                Total = Dbl(f, "total"),
                PaymentMethod = Str(f, "paymentMethod"),
                ShippingAddress = Str(f, "shippingAddress"),
                Items = items
            };
        }

        private static string ParseFirebaseError(string json)
        {
            try
            {
                return JsonDocument.Parse(json).RootElement
                    .GetProperty("error")
                    .GetProperty("message")
                    .GetString() ?? "Unknown error";
            }
            catch { return json; }
        }
    }

    // ── Category model ───────────────────────────────────────────────────────
    public class Category
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IconName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}