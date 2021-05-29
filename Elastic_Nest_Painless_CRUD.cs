// This example is supposed to provide CRUD capabilities to whoever uses ElasticSearch; it catches a modification within a list that's within an object (i.e. the object runs a separate save compared to the list component)
// The methods should be called to a "data shipper" while maintaining DataShipper sub-models, conversions, and tests
// The code is somewhat in pseudocode but uses "NEST, C#, and the "Painless" language of ElasticSearch

// creates a new index when sub Model is saved
public async Task CreatedSubModel<TModel>(TModel obj, string indexName)
    {
    Args.VerifyNotNull(obj, nameof(obj));
    Args.VerifyNotNull(indexName, nameof(indexName));
    Args.VerifyNotEmpty(indexName, nameof(indexName));

    var client = new ElasticClient();
    var mainShipper = new MainShipper();
    var retrievedModel = new RetrievedModel();
    var subModel = new List<NameofSubModelShipper>();

    var retrievedMainModel = await mModel.GetAsync(retrievedModel.Id);

      // retrieve the Main object while creating the subModel
      // note the index id must be converted to a string and be concatenated to the id for proper setup
    client.Update<MainShipper, object>("main_" + retrievedMainModel.Id, q => q
        .Index("name_of_index_shipping_to")
        .Script(script =>
            (script.Source("if (ctx._source.subModel == null)
            { ctx._source.subModel = elem; } 
            else 
            { ctx._source.subModels += subModel; }"))
        .Params(d => d
            .Add("subModel", subModel )
        )
    ));
    }

// updates the index when a subModel is updated
public async Task UpdateSubModel<TModel>(TModel obj, string indexName)
    {
    Args.VerifyNotNull(obj, nameof(obj));
    Args.VerifyNotNull(indexName, nameof(indexName));
    Args.VerifyNotEmpty(indexName, nameof(indexName));

    var client = new ElasticClient();
    var mainShipper = new MainShipper();
    var retrievedModel = new RetrievedModel();
    var subModel = new List<NameofSubModelShipper>();

    var retrievedMainModel = await mModel.GetAsync(retrievedModel.Id);

    // retrieve the the Main object while updating the subModel
    // properties must be setup to account for the local changes
    // updates in ElasticSearch re-indexes the sub list 
    client.Update<MainShipper, object>("main_" + retrievedMainModel.Id, q => q
        .Index("name_of_index_shipping_to")
        .Script(script =>
            (script.Source("if (ctx._source['subModel'] != null) 
            { for (item in ctx._source['subModels]) 
            { if (item.subModels.Id == params.id_param)  
            {item.subModels.Name = params.Name_param, 
            item.subModels.Title = params.Title_param, 
            item.subModels.Date = params.Date_param}} "))
        .Params(p => p
        .Add("subModel", subModel))
    ));
    }

// update the main shipper while deleting its sub Model
public async Task DeleteSubModel<TModel>(TModel obj, string indexName)
    {
    Args.VerifyNotNull(obj, nameof(obj));
    Args.VerifyNotNull(indexName, nameof(indexName));
    Args.VerifyNotEmpty(indexName, nameof(indexName));

    var client = new ElasticClient();
    var mainShipper = new MainShipper();
    var retrievedModel = new RetrievedModel();

var retrievedMainModel = await mModel.GetAsync(retrievedModel.Id);

    // retrieve the main model from the business layer in order to delete its index
    client.Update<MainShipper, object>("main_" + retrievedMainModel.Id, q => q
        .Index("name_of_index_shipping_to")
        .Script(script =>
            (script.Source("if (ctx._source['subModel'] != null) 
            { for (int i=ctx._source['subModels'].length-1; i>=0; i--) 
            { if (ctx._source['subModels'][i].Id == params.value_to_remove) 
            {ctx._source['subModels'].remove(i) 
            }}}"
        ))
    ));
    }
}



